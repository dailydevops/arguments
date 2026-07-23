namespace NetEvolve.Arguments.Analyser.Tests.Unit;

public sealed class ThrowIfDefaultAnalyzerTests
{
    [Test]
    [Arguments("argument.Equals(default)")]
    [Arguments("argument == default")]
    [Arguments("default == argument")]
    [Arguments("argument.Equals(default(Guid))")]
    [Arguments("argument == default(Guid)")]
    public async Task Analyze_WhenDefaultCheckThrowsArgumentException_ReportsDiagnostic(string condition)
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

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfDefaultAnalyzer(), source);

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0004");
    }

    [Test]
    [Arguments("if (argument.Equals(default)) throw new ArgumentNullException(nameof(argument));")]
    [Arguments("if (argument.Equals(default)) throw new ArgumentException(\"custom\", nameof(argument));")]
    [Arguments("if (argument.Equals(1)) throw new ArgumentException(nameof(argument));")]
    [Arguments(
        """
            if (argument.Equals(default))
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

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfDefaultAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenParamNameIsStringLiteral_ReportsDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M(Guid argument)
                {
                    if (argument.Equals(default)) throw new ArgumentException("argument");
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfDefaultAnalyzer(), source);

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
    }

    [Test]
    public async Task CodeFix_WhenApplied_ReplacesWithThrowIfDefaultCall()
    {
        const string source = """
            using System;

            class C
            {
                void M(Guid argument)
                {
                    if (argument.Equals(default)) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfDefaultAnalyzer(),
            new ThrowIfDefaultCodeFixProvider(),
            source
        );

        _ = await Assert.That(fixedSource).Contains("ArgumentException.ThrowIfDefault(argument);");
    }
}
