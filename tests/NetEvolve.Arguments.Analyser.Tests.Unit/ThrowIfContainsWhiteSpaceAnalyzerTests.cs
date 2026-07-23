namespace NetEvolve.Arguments.Analyser.Tests.Unit;

public sealed class ThrowIfContainsWhiteSpaceAnalyzerTests
{
    [Test]
    [Arguments("argument.Any(c => char.IsWhiteSpace(c))")]
    [Arguments("argument.Any(char.IsWhiteSpace)")]
    public async Task Analyze_WhenWhiteSpaceCheckThrowsArgumentException_ReportsDiagnosticAndFixes(string condition)
    {
        var source = $$"""
            using System;
            using System.Linq;

            class C
            {
                void M(string argument)
                {
                    if ({{condition}}) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfContainsWhiteSpaceAnalyzer(), source);

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0008");

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfContainsWhiteSpaceAnalyzer(),
            new ThrowIfContainsWhiteSpaceCodeFixProvider(),
            source
        );

        _ = await Assert.That(fixedSource).Contains("ArgumentException.ThrowIfContainsWhiteSpace(argument);");
    }

    [Test]
    public async Task Analyze_WhenThrowingArgumentNullException_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;
            using System.Linq;

            class C
            {
                void M(string argument)
                {
                    if (argument.Any(char.IsWhiteSpace)) throw new ArgumentNullException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfContainsWhiteSpaceAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }
}
