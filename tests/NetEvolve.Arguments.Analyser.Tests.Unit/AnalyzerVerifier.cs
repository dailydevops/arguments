namespace NetEvolve.Arguments.Analyser.Tests.Unit;

using System;
using System.Collections.Generic;
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
        bool useLegacyReferences = true,
        CancellationToken cancellationToken = default
    ) => GetDiagnosticsCoreAsync(analyzer, source, useLegacyReferences, cancellationToken: cancellationToken);

    public static Task<string> ApplyFixAsync(
        DiagnosticAnalyzer analyzer,
        CodeFixProvider codeFix,
        string source,
        bool useLegacyReferences = true,
        int expectedDiagnosticCount = 1,
        CancellationToken cancellationToken = default
    ) => ApplyFixCoreAsync(analyzer, codeFix, source, useLegacyReferences, expectedDiagnosticCount, cancellationToken);

    /// <summary>
    /// Applies the code fix's <see cref="CodeFixProvider.GetFixAllProvider"/> (e.g. <c>WellKnownFixAllProviders.BatchFixer</c>)
    /// across every diagnostic the analyzer reports in the document, exercising the Fix All / Batch Fixer code path.
    /// </summary>
    public static Task<string> ApplyFixAllAsync(
        DiagnosticAnalyzer analyzer,
        CodeFixProvider codeFix,
        string source,
        bool useLegacyReferences = true,
        CancellationToken cancellationToken = default
    ) => ApplyFixAllCoreAsync(analyzer, codeFix, source, useLegacyReferences, cancellationToken: cancellationToken);

    private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsCoreAsync(
        DiagnosticAnalyzer analyzer,
        string source,
        bool useLegacyReferences,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var compilation = CreateCompilation(source, useLegacyReferences);
        var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));

        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<string> ApplyFixCoreAsync(
        DiagnosticAnalyzer analyzer,
        CodeFixProvider codeFix,
        string source,
        bool useLegacyReferences,
        int expectedDiagnosticCount,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

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

        var compilation = (CSharpCompilation)
            (await document.Project.GetCompilationAsync(cancellationToken: cancellationToken).ConfigureAwait(false))!;
        var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));
        var diagnostics = await compilationWithAnalyzers
            .GetAnalyzerDiagnosticsAsync(CancellationToken.None)
            .ConfigureAwait(false);

        if (diagnostics.Length != expectedDiagnosticCount)
        {
            throw new InvalidOperationException(
                $"Expected {expectedDiagnosticCount} diagnostic(s) but found {diagnostics.Length}."
            );
        }

        // Only the first diagnostic is fixed here; callers that pass expectedDiagnosticCount > 1 (to test a fix
        // applied to one site among several matches) still only exercise the first one. Exercising every matched
        // site in one go requires ApplyFixAllAsync instead.
        var diagnostic = diagnostics[0];

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
        var newRoot = await newDocument.GetSyntaxRootAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return newRoot!.ToFullString();
    }

    private static async Task<string> ApplyFixAllCoreAsync(
        DiagnosticAnalyzer analyzer,
        CodeFixProvider codeFix,
        string source,
        bool useLegacyReferences,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

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

        var diagnostics = await GetAnalyzerDiagnosticsAsync(document, analyzer, CancellationToken.None)
            .ConfigureAwait(false);

        if (diagnostics.Length < 2)
        {
            throw new InvalidOperationException(
                "Expected at least two diagnostics to exercise the Fix All / Batch Fixer code path."
            );
        }

        var equivalenceKey = await GetEquivalenceKeyAsync(document, codeFix, diagnostics[0], cancellationToken)
            .ConfigureAwait(false);

        var fixAllProvider =
            codeFix.GetFixAllProvider()
            ?? throw new InvalidOperationException("The code fix provider does not support Fix All.");

        var fixAllContext = new FixAllContext(
            document,
            codeFix,
            FixAllScope.Document,
            equivalenceKey,
            codeFix.FixableDiagnosticIds,
            new AnalyzerFixAllDiagnosticProvider(analyzer),
            CancellationToken.None
        );

        var fixAllAction =
            await fixAllProvider.GetFixAsync(fixAllContext).ConfigureAwait(false)
            ?? throw new InvalidOperationException("No Fix All action was registered.");

        var operations = await fixAllAction.GetOperationsAsync(CancellationToken.None).ConfigureAwait(false);
        var applyChanges = operations.OfType<ApplyChangesOperation>().Single();
        var newDocument = applyChanges.ChangedSolution.GetDocument(document.Id)!;
        var newRoot = await newDocument.GetSyntaxRootAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return newRoot!.ToFullString();
    }

    private static async Task<string?> GetEquivalenceKeyAsync(
        Document document,
        CodeFixProvider codeFix,
        Diagnostic diagnostic,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        CodeAction? registeredAction = null;
        var fixContext = new CodeFixContext(
            document,
            diagnostic,
            (action, _) => registeredAction ??= action,
            CancellationToken.None
        );

        await codeFix.RegisterCodeFixesAsync(fixContext).ConfigureAwait(false);

        return registeredAction?.EquivalenceKey;
    }

    private static async Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(
        Document document,
        DiagnosticAnalyzer analyzer,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var compilation = (CSharpCompilation)
            (await document.Project.GetCompilationAsync(cancellationToken).ConfigureAwait(false))!;
        var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));

        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Feeds the analyzer's own diagnostics back into the Fix All engine, since the in-memory test project has no diagnostics of its own otherwise.</summary>
    private sealed class AnalyzerFixAllDiagnosticProvider(DiagnosticAnalyzer analyzer)
        : FixAllContext.DiagnosticProvider
    {
        public override async Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(
            Project project,
            CancellationToken cancellationToken
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            var diagnostics = ImmutableArray<Diagnostic>.Empty;

            foreach (var document in project.Documents)
            {
                diagnostics = diagnostics.AddRange(
                    await GetAnalyzerDiagnosticsAsync(document, analyzer, cancellationToken).ConfigureAwait(false)
                );
            }

            return diagnostics;
        }

        public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(
            Document document,
            CancellationToken cancellationToken
        ) => GetAllDiagnosticsForDocumentAsync(document, cancellationToken);

        public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(
            Project project,
            CancellationToken cancellationToken
        ) => GetAllDiagnosticsAsync(project, cancellationToken);

        private async Task<IEnumerable<Diagnostic>> GetAllDiagnosticsForDocumentAsync(
            Document document,
            CancellationToken cancellationToken
        ) => await GetAnalyzerDiagnosticsAsync(document, analyzer, cancellationToken).ConfigureAwait(false);
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
