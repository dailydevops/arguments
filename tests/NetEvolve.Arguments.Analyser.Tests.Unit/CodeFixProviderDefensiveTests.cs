namespace NetEvolve.Arguments.Analyser.Tests.Unit;

using System.Threading;
using Microsoft.CodeAnalysis.CodeFixes;

public sealed class CodeFixProviderDefensiveTests
{
    private const string Source = """
        class C
        {
            void M(bool argument)
            {
                if (argument) { }
            }
        }
        """;

    [Test]
    [Arguments(typeof(ThrowIfCountCodeFixProvider))]
    [Arguments(typeof(ThrowIfLengthCodeFixProvider))]
    [Arguments(typeof(ThrowIfNullOrEmptyCodeFixProvider))]
    [Arguments(typeof(ThrowIfOutOfRangeCodeFixProvider))]
    public async Task RegisterCodeFixes_WhenDiagnosticIsOutsideIfStatement_RegistersNothing(
        Type providerType,
        CancellationToken cancellationToken = default
    )
    {
        var provider = (CodeFixProvider)Activator.CreateInstance(providerType)!;

        var count = await AnalyzerVerifier.CountRegisteredFixesAsync(provider, Source, "void M", cancellationToken);

        _ = await Assert.That(count).IsEqualTo(0);
    }

    [Test]
    [Arguments(typeof(ThrowIfCountCodeFixProvider))]
    [Arguments(typeof(ThrowIfLengthCodeFixProvider))]
    [Arguments(typeof(ThrowIfNullOrEmptyCodeFixProvider))]
    [Arguments(typeof(ThrowIfOutOfRangeCodeFixProvider))]
    public async Task RegisterCodeFixes_WhenIfConditionDoesNotMatchPattern_RegistersNothing(
        Type providerType,
        CancellationToken cancellationToken = default
    )
    {
        var provider = (CodeFixProvider)Activator.CreateInstance(providerType)!;

        var count = await AnalyzerVerifier.CountRegisteredFixesAsync(
            provider,
            Source,
            "if (argument)",
            cancellationToken
        );

        _ = await Assert.That(count).IsEqualTo(0);
    }
}
