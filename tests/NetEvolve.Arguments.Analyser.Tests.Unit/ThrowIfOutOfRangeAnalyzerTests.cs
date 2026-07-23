namespace NetEvolve.Arguments.Analyser.Tests.Unit;

public sealed class ThrowIfOutOfRangeAnalyzerTests
{
    [Test]
    [Arguments("argument < 0", "ThrowIfNegative(argument);")]
    [Arguments("argument <= 0", "ThrowIfNegativeOrZero(argument);")]
    [Arguments("argument == 0", "ThrowIfZero(argument);")]
    [Arguments("argument < 42", "ThrowIfLessThan(argument, 42);")]
    [Arguments("argument <= 42", "ThrowIfLessThanOrEqual(argument, 42);")]
    [Arguments("argument > 42", "ThrowIfGreaterThan(argument, 42);")]
    [Arguments("argument >= 42", "ThrowIfGreaterThanOrEqual(argument, 42);")]
    [Arguments("argument == 42", "ThrowIfEqual(argument, 42);")]
    [Arguments("argument != 42", "ThrowIfNotEqual(argument, 42);")]
    public async Task Analyze_WhenComparisonThrowsArgumentOutOfRangeException_ReportsDiagnosticAndFixes(
        string condition,
        string expectedInvocation
    )
    {
        var source = $$"""
            using System;

            class C
            {
                void M(int argument)
                {
                    if ({{condition}}) throw new ArgumentOutOfRangeException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfOutOfRangeAnalyzer(), source);

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0003");

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            new ThrowIfOutOfRangeCodeFixProvider(),
            source
        );

        _ = await Assert.That(fixedSource).Contains($"ArgumentOutOfRangeException.{expectedInvocation}");
    }

    [Test]
    public async Task Analyze_WhenValueOperandIsLiteral_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M(int argument)
                {
                    if (0 > argument) throw new ArgumentOutOfRangeException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfOutOfRangeAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenThrowingArgumentException_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M(int argument)
                {
                    if (argument < 0) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfOutOfRangeAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenBuiltInThrowIfNegativeAvailable_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M(int argument)
                {
                    if (argument < 0) throw new ArgumentOutOfRangeException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            source,
            useLegacyReferences: false
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenCombinedRangeThrowsArgumentOutOfRangeException_ReportsDiagnosticAndFixes()
    {
        const string source = """
            using System;

            class C
            {
                void M(int argument)
                {
                    if (argument < 5 || argument > 100) throw new ArgumentOutOfRangeException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfOutOfRangeAnalyzer(), source);

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0003");

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            new ThrowIfOutOfRangeCodeFixProvider(),
            source
        );

        _ = await Assert.That(fixedSource).Contains("ArgumentOutOfRangeException.ThrowIfOutOfRange(argument, 5, 100);");
    }

    [Test]
    public async Task Analyze_WhenIfHasElseClause_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M(int argument)
                {
                    if (argument < 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(argument));
                    }
                    else
                    {
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfOutOfRangeAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenExceptionHasTooManyArguments_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M(int argument)
                {
                    if (argument < 0) throw new ArgumentOutOfRangeException(nameof(argument), argument, "msg", 1);
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfOutOfRangeAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    [Arguments("bool argument", "argument")]
    [Arguments("int argument, int other", "argument < 5 || other > 100")]
    [Arguments("int argument", "argument > 100 || argument < 5")]
    [Arguments("int argument", "argument == 5 || argument > 100")]
    public async Task Analyze_WhenConditionIsUnrecognizedShape_DoesNotReportDiagnostic(
        string parameters,
        string condition
    )
    {
        var source = $$"""
            using System;

            class C
            {
                void M({{parameters}})
                {
                    if ({{condition}}) throw new ArgumentOutOfRangeException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfOutOfRangeAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }
}
