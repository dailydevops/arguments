namespace NetEvolve.Arguments.Analyser.Tests.Unit;

public sealed class ThrowIfLengthAnalyzerTests
{
    [Test]
    [Arguments("argument.Length > 100", "ThrowIfLengthGreaterThan(argument, 100);")]
    [Arguments("argument.Length < 5", "ThrowIfLengthLessThan(argument, 5);")]
    [Arguments("argument.Length < 5 || argument.Length > 100", "ThrowIfLengthOutOfRange(argument, 5, 100);")]
    public async Task Analyze_WhenLengthComparisonThrowsArgumentException_ReportsDiagnosticAndFixes(
        string condition,
        string expectedInvocation
    )
    {
        var source = $$"""
            using System;

            class C
            {
                void M(string argument)
                {
                    if ({{condition}}) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfLengthAnalyzer(), source);

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0006");

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfLengthAnalyzer(),
            new ThrowIfLengthCodeFixProvider(),
            source
        );

        _ = await Assert.That(fixedSource).Contains($"ArgumentException.{expectedInvocation}");
    }

    [Test]
    public async Task Analyze_WhenThrowingArgumentOutOfRangeException_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M(string argument)
                {
                    if (argument.Length > 100) throw new ArgumentOutOfRangeException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfLengthAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }
}
