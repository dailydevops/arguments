namespace NetEvolve.Arguments.Analyser.Tests.Unit;

public sealed class ThrowIfCountAnalyzerTests
{
    [Test]
    [Arguments("argument.Count > 100", "ThrowIfCountGreaterThan(argument, 100);")]
    [Arguments("argument.Count < 5", "ThrowIfCountLessThan(argument, 5);")]
    [Arguments("argument.Count < 5 || argument.Count > 100", "ThrowIfCountOutOfRange(argument, 5, 100);")]
    [Arguments("argument.Count() > 100", "ThrowIfCountGreaterThan(argument, 100);")]
    public async Task Analyze_WhenCountComparisonThrowsArgumentException_ReportsDiagnosticAndFixes(
        string condition,
        string expectedInvocation
    )
    {
        var source = $$"""
            using System;
            using System.Collections.Generic;
            using System.Linq;

            class C
            {
                void M(ICollection<int> argument)
                {
                    if ({{condition}}) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfCountAnalyzer(), source);

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0007");

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfCountAnalyzer(),
            new ThrowIfCountCodeFixProvider(),
            source
        );

        _ = await Assert.That(fixedSource).Contains($"ArgumentException.{expectedInvocation}");
    }

    [Test]
    public async Task Analyze_WhenThrowingArgumentNullException_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;
            using System.Collections.Generic;

            class C
            {
                void M(ICollection<int> argument)
                {
                    if (argument.Count > 100) throw new ArgumentNullException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfCountAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }
}
