namespace NetEvolve.Arguments.Analyser.Tests.Unit;

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

/// <summary>
/// Compiles test sources against two reference sets: the modern .NET runtime (where BCL throw-helpers
/// already exist, so the analyzers under test must stay silent) and the netstandard2.1 reference
/// assembly (where they don't, simulating the frameworks NetEvolve.Arguments polyfills).
/// </summary>
internal static class AnalyzerVerifier
{
    private static readonly ImmutableArray<MetadataReference> ModernReferences = CreateModernReferences();
    private static readonly Lazy<ImmutableArray<MetadataReference>> LegacyReferences = new(CreateLegacyReferences);

    public static Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(
        DiagnosticAnalyzer analyzer,
        string source,
        bool useLegacyReferences = true
    ) => GetDiagnosticsCoreAsync(analyzer, source, useLegacyReferences);

    public static Task<string> ApplyFixAsync(
        DiagnosticAnalyzer analyzer,
        CodeFixProvider codeFix,
        string source,
        bool useLegacyReferences = true
    ) => ApplyFixCoreAsync(analyzer, codeFix, source, useLegacyReferences);

    private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsCoreAsync(
        DiagnosticAnalyzer analyzer,
        string source,
        bool useLegacyReferences
    )
    {
        var compilation = CreateCompilation(source, useLegacyReferences);
        var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));

        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(CancellationToken.None).ConfigureAwait(false);
    }

    private static async Task<string> ApplyFixCoreAsync(
        DiagnosticAnalyzer analyzer,
        CodeFixProvider codeFix,
        string source,
        bool useLegacyReferences
    )
    {
        using var workspace = new AdhocWorkspace();

        var initialProject = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var configuredProject = initialProject
            .WithMetadataReferences(useLegacyReferences ? LegacyReferences.Value : ModernReferences)
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        if (!workspace.TryApplyChanges(configuredProject.Solution))
        {
            throw new InvalidOperationException("Failed to apply project configuration to the workspace.");
        }

        var document = workspace.AddDocument(configuredProject.Id, "Test.cs", SourceText.From(source));

        var compilation = (CSharpCompilation)(await document.Project.GetCompilationAsync().ConfigureAwait(false))!;
        var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));
        var diagnostics = await compilationWithAnalyzers
            .GetAnalyzerDiagnosticsAsync(CancellationToken.None)
            .ConfigureAwait(false);

        var diagnostic = diagnostics.Single();

        CodeAction? registeredAction = null;
        var fixContext = new CodeFixContext(
            document,
            diagnostic,
            (action, _) => registeredAction ??= action,
            CancellationToken.None
        );

        await codeFix.RegisterCodeFixesAsync(fixContext).ConfigureAwait(false);

        if (registeredAction is null)
        {
            throw new InvalidOperationException("No code fix action was registered.");
        }

        var operations = await registeredAction.GetOperationsAsync(CancellationToken.None).ConfigureAwait(false);
        var applyChanges = operations.OfType<ApplyChangesOperation>().Single();
        var newDocument = applyChanges.ChangedSolution.GetDocument(document.Id)!;
        var newRoot = await newDocument.GetSyntaxRootAsync().ConfigureAwait(false);

        return newRoot!.ToFullString();
    }

    private static CSharpCompilation CreateCompilation(string source, bool useLegacyReferences)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest));

        return CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            useLegacyReferences ? LegacyReferences.Value : ModernReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
    }

    private static ImmutableArray<MetadataReference> CreateModernReferences()
    {
        var trustedAssemblies = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trustedAssemblies
            .Split(Path.PathSeparator)
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
            .ToImmutableArray();
    }

    private static ImmutableArray<MetadataReference> CreateLegacyReferences() =>
        ImmutableArray.Create<MetadataReference>(MetadataReference.CreateFromFile(FindNetStandardReferenceAssembly()));

    private static string FindNetStandardReferenceAssembly()
    {
        var runtimeDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var dotnetRoot = Directory.GetParent(runtimeDirectory)!.Parent!.Parent!.FullName;
        var packRoot = Path.Combine(dotnetRoot, "packs", "NETStandard.Library.Ref");

        if (!Directory.Exists(packRoot))
        {
            throw new InvalidOperationException(
                $"NETStandard.Library.Ref pack not found at '{packRoot}'. Install it via the .NET SDK workload/pack manager."
            );
        }

        var versionDirectory = Directory
            .GetDirectories(packRoot)
            .OrderByDescending(directory => directory, StringComparer.OrdinalIgnoreCase)
            .First();

        var targetFrameworkDirectory = Directory.GetDirectories(Path.Combine(versionDirectory, "ref"))[0];
        var netstandardDll = Path.Combine(targetFrameworkDirectory, "netstandard.dll");

        if (!File.Exists(netstandardDll))
        {
            throw new InvalidOperationException($"netstandard.dll reference assembly not found under '{packRoot}'.");
        }

        return netstandardDll;
    }
}
