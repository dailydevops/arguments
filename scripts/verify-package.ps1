#Requires -Version 7.0
<#
.SYNOPSIS
    End-to-end verification that the packed NetEvolve.Arguments NuGet package actually
    delivers a working Roslyn analyzer to a consumer.

.DESCRIPTION
    This script exists because NetEvolve.Arguments.Analyser previously shipped its assembly
    to `lib/netstandard2.0/` instead of `analyzers/dotnet/cs/`. NuGet treated it as an ordinary
    compile/runtime reference, Roslyn never loaded it, and none of the NEA00xx rules ever ran
    for a consumer - while the package installed successfully and the documentation advertised
    the rules. Nothing in the build or test suite caught this; it was found by manual inspection.

    The script performs a genuine end-to-end check:
      1. Packs src/NetEvolve.Arguments with a throwaway version into an isolated temp folder.
      2. Inspects the produced .nupkg and asserts:
           - exactly one analyzers/dotnet/cs/NetEvolve.Arguments.Analyser.dll entry
           - a lib/<tfm>/NetEvolve.Arguments.dll entry for every target framework the project
             declares
           - no analyser assembly anywhere under lib/
      3. Generates a throwaway consumer project (outside the repository and outside the git
         worktree, so no ambient Directory.Build.props / .editorconfig from this repo applies)
         that references the packed package from a local, `<clear/>`-only NuGet feed.
      4. The consumer contains code that triggers NEA0006 and NEA0009 (rules with no BCL gate,
         so they fire on every target framework), and an .editorconfig that raises both rules
         from their shipped `Info` severity to `warning`, because `dotnet build` does not
         surface Info-level diagnostics by default. Without this bump the check would pass
         vacuously.
      5. Builds the consumer for each requested target framework and fails loudly unless both
         NEA0006 and NEA0009 actually appear in the build output.

.PARAMETER Configuration
    Build configuration used for both the pack and the consumer build. Default: Release.

.PARAMETER PackageVersion
    Throwaway version stamped onto the packed package. Default: 1.0.0-packtest.

.PARAMETER ConsumerTargetFrameworks
    Target frameworks the throwaway consumer project multi-targets. Pick at least two so a
    TFM-specific regression is caught. Default: net10.0, net472 (a modern TFM and a .NET
    Framework TFM - the two ends of the support matrix).

.PARAMETER KeepTempFiles
    Do not delete the temporary working directory afterwards. Useful when debugging this
    script itself.

.EXAMPLE
    pwsh ./scripts/verify-package.ps1

.EXAMPLE
    pwsh ./scripts/verify-package.ps1 -ConsumerTargetFrameworks net10.0,net8.0,netstandard2.0,net472
#>
[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [string]$PackageVersion = '1.0.0-packtest',
    [string[]]$ConsumerTargetFrameworks = @('net10.0', 'net472'),
    [switch]$KeepTempFiles
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$scriptStart = Get-Date
$repoRoot = Split-Path -Parent $PSScriptRoot
$mainProject = Join-Path $repoRoot 'src/NetEvolve.Arguments/NetEvolve.Arguments.csproj'
$packageId = 'NetEvolve.Arguments'
$analyserAssemblyName = 'NetEvolve.Arguments.Analyser.dll'
$mainAssemblyName = 'NetEvolve.Arguments.dll'

if (-not (Test-Path $mainProject)) {
    throw "Cannot find project at '$mainProject'. Is this script still located at <repo>/scripts/verify-package.ps1?"
}

# Isolated temp root per run: pack output, consumer project AND the NuGet global-packages cache
# all live under here. Reusing a fixed pack directory or the ambient NuGet cache would let a
# stale, previously-restored good package mask a newly-broken one on a second run.
$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) "netevolve-pack-verify-$([guid]::NewGuid().ToString('N'))"
$packDir = Join-Path $tempRoot 'pack'
$consumerDir = Join-Path $tempRoot 'consumer'
$nugetCacheDir = Join-Path $tempRoot 'nuget-packages'

New-Item -ItemType Directory -Path $packDir -Force | Out-Null
New-Item -ItemType Directory -Path $consumerDir -Force | Out-Null
New-Item -ItemType Directory -Path $nugetCacheDir -Force | Out-Null

Write-Host "Working directory: $tempRoot"

$previousNugetPackages = $env:NUGET_PACKAGES

$failures = [System.Collections.Generic.List[string]]::new()
$exitCode = 1

function Invoke-Checked {
    param(
        [Parameter(Mandatory)][string]$Exe,
        [Parameter(Mandatory)][string[]]$ArgumentList,
        [Parameter(Mandatory)][string]$FailureMessage
    )

    Write-Host "+ $Exe $($ArgumentList -join ' ')"
    $output = & $Exe @ArgumentList 2>&1 | ForEach-Object { $_.ToString() }
    $output | ForEach-Object { Write-Host $_ }
    if ($LASTEXITCODE -ne 0) {
        throw "$FailureMessage (exit code $LASTEXITCODE)"
    }
    return $output
}

try {
    # 1. Restore, build and pack as separate, explicit steps into the isolated pack folder.
    # (Combining restore+build+pack into a single 'dotnet pack' invocation against a brand-new,
    # isolated NUGET_PACKAGES cache has been observed to race for this cross-targeting project -
    # the outer pack target can run before every inner per-TFM build has finished, producing a
    # spurious NU5026 "file not found" error. Explicit, separate steps avoid that.)
    Write-Host "`n=== Step 1: dotnet restore / build / pack ($PackageVersion) ==="
    Invoke-Checked -Exe 'dotnet' -FailureMessage 'dotnet restore failed' -ArgumentList @(
        'restore', $mainProject, '--nologo'
    ) | Out-Null
    Invoke-Checked -Exe 'dotnet' -FailureMessage 'dotnet build failed' -ArgumentList @(
        'build', $mainProject,
        '-c', $Configuration,
        '--no-restore',
        "-p:Version=$PackageVersion",
        '--nologo'
    ) | Out-Null
    Invoke-Checked -Exe 'dotnet' -FailureMessage 'dotnet pack failed' -ArgumentList @(
        'pack', $mainProject,
        '-c', $Configuration,
        '-o', $packDir,
        '--no-restore',
        '--no-build',
        "-p:Version=$PackageVersion",
        '--nologo'
    ) | Out-Null

    $nupkgPath = Join-Path $packDir "$packageId.$PackageVersion.nupkg"
    if (-not (Test-Path $nupkgPath)) {
        throw "Expected package '$nupkgPath' was not produced by 'dotnet pack'."
    }
    Write-Host "Produced package: $nupkgPath"

    # Determine the target frameworks the project actually declares, instead of hardcoding them,
    # so this check does not silently drift out of sync with Directory.Build.props.
    $tfmProperty = (Invoke-Checked -Exe 'dotnet' -FailureMessage 'Could not evaluate TargetFrameworks' -ArgumentList @(
        'msbuild', $mainProject, '-nologo', '-getProperty:TargetFrameworks'
    )) -join ''
    $expectedTfms = $tfmProperty.Trim() -split ';' | Where-Object { $_ -ne '' }
    if ($expectedTfms.Count -eq 0) {
        throw "Could not determine TargetFrameworks for '$mainProject' (got: '$tfmProperty')."
    }
    Write-Host "Expected target frameworks in package: $($expectedTfms -join ', ')"

    # 2. Inspect the .nupkg contents.
    Write-Host "`n=== Step 2: inspect package contents ==="
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::OpenRead($nupkgPath)
    try {
        $entryPaths = $zip.Entries | ForEach-Object { $_.FullName -replace '\\', '/' }
    }
    finally {
        $zip.Dispose()
    }

    $analyserEntries = $entryPaths | Where-Object {
        $_ -ieq "analyzers/dotnet/cs/$analyserAssemblyName"
    }
    if (@($analyserEntries).Count -ne 1) {
        $failures.Add("Expected exactly one 'analyzers/dotnet/cs/$analyserAssemblyName' entry, found $(@($analyserEntries).Count). Entries under analyzers/: $(($entryPaths | Where-Object { $_ -imatch '^analyzers/' }) -join ', ')")
    }
    else {
        Write-Host "OK: exactly one analyzers/dotnet/cs/$analyserAssemblyName"
    }

    $analyserUnderLib = $entryPaths | Where-Object { $_ -imatch '^lib/' -and $_ -imatch 'Analyser' }
    if (@($analyserUnderLib).Count -gt 0) {
        $failures.Add("Found analyser assembly leaking into lib/: $($analyserUnderLib -join ', ')")
    }
    else {
        Write-Host 'OK: no analyser assembly under lib/'
    }

    foreach ($tfm in $expectedTfms) {
        $expectedLibEntry = "lib/$tfm/$mainAssemblyName"
        $found = $entryPaths | Where-Object { $_ -ieq $expectedLibEntry }
        if (@($found).Count -ne 1) {
            $failures.Add("Expected exactly one '$expectedLibEntry' entry, found $(@($found).Count).")
        }
        else {
            Write-Host "OK: $expectedLibEntry"
        }
    }

    if ($failures.Count -gt 0) {
        throw "Package structure assertions failed:`n - $($failures -join "`n - ")"
    }

    # 3. Generate the throwaway consumer project.
    # NUGET_PACKAGES is pointed at a guaranteed-empty, isolated cache only from here on: the
    # consumer restore is what must never resolve the package from a stale global cache entry. The
    # pack step above intentionally used the normal machine-wide cache so it doesn't have to
    # re-download every GlobalPackageReference analyzer and targeting pack on each run.
    Write-Host "`n=== Step 3: generate throwaway consumer project ==="
    $env:NUGET_PACKAGES = $nugetCacheDir
    $tfmJoined = [string]::Join(';', $ConsumerTargetFrameworks)

    $csproj = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>$tfmJoined</TargetFrameworks>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="$packageId" Version="$PackageVersion" />
  </ItemGroup>
</Project>
"@
    Set-Content -Path (Join-Path $consumerDir 'ConsumerCheck.csproj') -Value $csproj -Encoding utf8

    # root = true so nothing from an ambient .editorconfig elsewhere on disk can interfere, and so
    # this file's severity bump is the only one in effect.
    $editorconfig = @"
root = true

[*.cs]
dotnet_diagnostic.NEA0006.severity = warning
dotnet_diagnostic.NEA0009.severity = warning
"@
    Set-Content -Path (Join-Path $consumerDir '.editorconfig') -Value $editorconfig -Encoding utf8

    # Source that MUST trigger NEA0006 (ThrowIfLength) and NEA0009 (ThrowIfEmptyGuid). Both rules
    # have no BCL-version gate, so they fire identically on every target framework, which is what
    # makes them suitable for a TFM matrix check.
    $source = @"
using System;

namespace ConsumerCheck;

public class Sample
{
    public void M(string argument)
    {
        if (argument.Length > 100)
        {
            throw new ArgumentException(nameof(argument));
        }
    }

    public void G(Guid argument)
    {
        if (argument == Guid.Empty)
        {
            throw new ArgumentException(nameof(argument));
        }
    }
}
"@
    Set-Content -Path (Join-Path $consumerDir 'Sample.cs') -Value $source -Encoding utf8

    # <clear/> plus only the local pack folder: proves the consumer resolves the package purely
    # from what we just packed, with no fallback to nuget.org (or a stale cached copy) masking a
    # broken package.
    $packDirForNuget = $packDir -replace '\\', '/'
    $nugetConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local-packtest" value="$packDirForNuget" />
  </packageSources>
</configuration>
"@
    Set-Content -Path (Join-Path $consumerDir 'nuget.config') -Value $nugetConfig -Encoding utf8

    # 4. Build the consumer once per target framework and assert both diagnostics fire.
    Write-Host "`n=== Step 4: build consumer per target framework and check diagnostics ==="
    $consumerProject = Join-Path $consumerDir 'ConsumerCheck.csproj'
    $diagnosticFailures = [System.Collections.Generic.List[string]]::new()

    foreach ($tfm in $ConsumerTargetFrameworks) {
        Write-Host "`n--- building consumer for $tfm ---"
        Remove-Item -Path (Join-Path $consumerDir 'bin'), (Join-Path $consumerDir 'obj') -Recurse -Force -ErrorAction SilentlyContinue

        & dotnet build $consumerProject -c $Configuration -f $tfm --nologo 2>&1 |
            Tee-Object -Variable buildOutputLines | ForEach-Object { Write-Host $_ }
        $buildExitCode = $LASTEXITCODE
        $buildOutput = ($buildOutputLines -join "`n")

        if ($buildExitCode -ne 0) {
            $diagnosticFailures.Add("[$tfm] consumer build failed outright (exit code $buildExitCode) - see output above.")
            continue
        }

        $hasNea0006 = $buildOutput -match 'NEA0006'
        $hasNea0009 = $buildOutput -match 'NEA0009'

        if (-not $hasNea0006) {
            $diagnosticFailures.Add("[$tfm] build succeeded but NEA0006 did not appear in the output - the analyzer is not running for this target framework.")
        }
        if (-not $hasNea0009) {
            $diagnosticFailures.Add("[$tfm] build succeeded but NEA0009 did not appear in the output - the analyzer is not running for this target framework.")
        }
        if ($hasNea0006 -and $hasNea0009) {
            Write-Host "OK [$tfm]: both NEA0006 and NEA0009 fired."
        }
    }

    if ($diagnosticFailures.Count -gt 0) {
        throw "Analyzer diagnostic assertions failed:`n - $($diagnosticFailures -join "`n - ")"
    }

    Write-Host "`n=== All package verification checks passed ($([Math]::Round(((Get-Date) - $scriptStart).TotalSeconds, 1))s) ==="
    $exitCode = 0
}
catch {
    Write-Host "`n=== PACKAGE VERIFICATION FAILED ===" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    $exitCode = 1
}
finally {
    if ($previousNugetPackages) {
        $env:NUGET_PACKAGES = $previousNugetPackages
    }
    else {
        Remove-Item Env:\NUGET_PACKAGES -ErrorAction SilentlyContinue
    }

    if ($KeepTempFiles) {
        Write-Host "Keeping temp directory for inspection: $tempRoot"
    }
    else {
        Remove-Item -Path $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}

exit $exitCode
