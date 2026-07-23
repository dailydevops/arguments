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
    [Arguments("if (argument.Any(char.IsWhiteSpace)) throw new ArgumentNullException(nameof(argument));")]
    [Arguments("if (argument.Any(c => c == ' ')) throw new ArgumentException(nameof(argument));")]
    [Arguments("if (argument.Any(c => char.IsWhiteSpace(other))) throw new ArgumentException(nameof(argument));")]
    [Arguments("if (argument.Contains(' ')) throw new ArgumentException(nameof(argument));")]
    [Arguments(
        """
            if (argument.Any(char.IsWhiteSpace))
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
            using System.Linq;

            class C
            {
                void M(string argument, char other)
                {
                    {{statement}}
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfContainsWhiteSpaceAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }
}
