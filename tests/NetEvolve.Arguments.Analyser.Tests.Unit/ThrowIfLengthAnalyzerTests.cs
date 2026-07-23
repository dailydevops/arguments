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
    [Arguments("if (argument.Length > 100) throw new ArgumentOutOfRangeException(nameof(argument));")]
    [Arguments("if (argument.Count > 100) throw new ArgumentException(nameof(argument));")]
    [Arguments("if (argument.Length == 100) throw new ArgumentException(nameof(argument));")]
    [Arguments("if (argument.Length < 5 || other.Length > 100) throw new ArgumentException(nameof(argument));")]
    [Arguments(
        """
            if (argument.Length > 100)
            {
                throw new ArgumentException(nameof(argument));
            }
            else
            {
            }
            """
    )]
    public async Task Analyze_WhenConditionOrExceptionIsNotRecognized_DoesNotReportDiagnostic(string statement)
    {
        var source = $$"""
            using System;

            class C
            {
                void M(string argument, string other)
                {
                    {{statement}}
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfLengthAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }
}
