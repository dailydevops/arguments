namespace NetEvolve.Arguments.Analyser.Tests.Unit;

public sealed class ThrowIfEmptyGuidAnalyzerTests
{
    [Test]
    [Arguments("argument == Guid.Empty")]
    [Arguments("Guid.Empty == argument")]
    [Arguments("argument.Equals(Guid.Empty)")]
    public async Task Analyze_WhenEmptyGuidCheckThrowsArgumentException_ReportsDiagnostic(string condition)
    {
        var source = $$"""
            using System;

            class C
            {
                void M(Guid argument)
                {
                    if ({{condition}}) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfEmptyGuidAnalyzer(), source);

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0009");
    }

    [Test]
    [Arguments("if (argument == Guid.Empty) throw new ArgumentNullException(nameof(argument));")]
    [Arguments("if (argument == Guid.Empty) throw new ArgumentException(\"custom\", nameof(argument));")]
    [Arguments("if (argument == Guid.NewGuid()) throw new ArgumentException(nameof(argument));")]
    [Arguments(
        """
            if (argument == Guid.Empty)
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
                void M(Guid argument)
                {
                    {{statement}}
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfEmptyGuidAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task CodeFix_WhenApplied_ReplacesWithThrowIfEmptyGuidCall()
    {
        const string source = """
            using System;

            class C
            {
                void M(Guid argument)
                {
                    if (argument == Guid.Empty) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfEmptyGuidAnalyzer(),
            new ThrowIfEmptyGuidCodeFixProvider(),
            source
        );

        _ = await Assert.That(fixedSource).Contains("ArgumentException.ThrowIfEmptyGuid(argument);");
    }
}
