namespace NetEvolve.Arguments.Analyser.Tests.Unit;

using System.Threading;

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
        string expectedInvocation,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

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

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0003");

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            new ThrowIfOutOfRangeCodeFixProvider(),
            source,
            cancellationToken: cancellationToken
        );

        var expected = $$"""
            using System;

            class C
            {
                void M(int argument)
                {
                    ArgumentOutOfRangeException.{{expectedInvocation}}
                }
            }
            """;

        _ = await Assert.That(fixedSource).IsEqualTo(expected);
    }

    [Test]
    public async Task Analyze_WhenBoundIsNotALiteral_ReportsDiagnosticAndFixes(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string source = """
            using System;

            class C
            {
                void M(int argument, int other)
                {
                    if (argument < other) throw new ArgumentOutOfRangeException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            new ThrowIfOutOfRangeCodeFixProvider(),
            source,
            cancellationToken: cancellationToken
        );

        const string expected = """
            using System;

            class C
            {
                void M(int argument, int other)
                {
                    ArgumentOutOfRangeException.ThrowIfLessThan(argument, other);
                }
            }
            """;

        _ = await Assert.That(fixedSource).IsEqualTo(expected);
    }

    [Test]
    public async Task Analyze_WhenValueOperandIsLiteral_DoesNotReportDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

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

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenThrowingArgumentException_DoesNotReportDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

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

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenBuiltInThrowIfNegativeAvailable_DoesNotReportDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

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
            useLegacyReferences: false,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenCombinedRangeThrowsArgumentOutOfRangeException_ReportsDiagnosticAndFixes(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

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

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0003");

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            new ThrowIfOutOfRangeCodeFixProvider(),
            source,
            cancellationToken: cancellationToken
        );

        const string expected = """
            using System;

            class C
            {
                void M(int argument)
                {
                    ArgumentOutOfRangeException.ThrowIfOutOfRange(argument, 5, 100);
                }
            }
            """;

        _ = await Assert.That(fixedSource).IsEqualTo(expected);
    }

    [Test]
    public async Task Analyze_WhenCombinedRangeOperandIsDouble_DoesNotReportDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string source = """
            using System;

            class C
            {
                void M(double argument)
                {
                    if (argument < 5 || argument > 100) throw new ArgumentOutOfRangeException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    [Arguments("argument < 42")]
    [Arguments("argument <= 42")]
    [Arguments("argument > 42")]
    [Arguments("argument >= 42")]
    public async Task Analyze_WhenRelationalOperandIsFloat_DoesNotReportDiagnostic(
        string condition,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var source = $$"""
            using System;

            class C
            {
                void M(float argument)
                {
                    if ({{condition}}) throw new ArgumentOutOfRangeException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenEqualityOperandIsDouble_ReportsDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string source = """
            using System;

            class C
            {
                void M(double argument)
                {
                    if (argument == 42) throw new ArgumentOutOfRangeException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0003");
    }

    [Test]
    public async Task Analyze_WhenIfHasElseClause_DoesNotReportDiagnostic(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

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

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenExceptionHasTooManyArguments_DoesNotReportDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

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

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenCombinedRangeOperandIsInvocation_DoesNotReportDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string source = """
            using System;

            class C
            {
                int Next() => 0;

                void M()
                {
                    if (Next() < 5 || Next() > 100) throw new ArgumentOutOfRangeException("x");
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenCombinedRangeMemberAccessOperandNamesRootParameter_DoesNotReportDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        // The compared operand is the member-access chain "argument.Length", but the constructor here names
        // only the root parameter ("argument"). Rewriting to the throw-helper would capture the whole chain
        // via [CallerArgumentExpression] and change the reported ParamName from "argument" to "argument.Length",
        // so this must not be reported even though the shape is otherwise a recognized combined range.
        const string source = """
            using System;

            class C
            {
                void M(string argument)
                {
                    if (argument.Length < 5 || argument.Length > 100) throw new ArgumentOutOfRangeException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenParamNameArgumentDoesNotMatchComparedValue_DoesNotReportDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string source = """
            using System;

            class C
            {
                void M(int argument, int other)
                {
                    if (other < argument) throw new ArgumentOutOfRangeException(nameof(argument), "must not exceed");
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenExceptionArgumentIsUnrelatedToComparedValue_DoesNotReportDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string source = """
            using System;

            class C
            {
                void M()
                {
                    if (DateTime.Now.Hour < 9) throw new ArgumentOutOfRangeException("time");
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenCombinedRangeOperandIsElementAccess_DoesNotReportDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string source = """
            using System;

            class C
            {
                void M(int[] items)
                {
                    if (items[0] < 5 || items[0] > 100) throw new ArgumentOutOfRangeException("x");
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenExceptionHasActualValueAndMessage_DoesNotReportDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string source = """
            using System;

            class C
            {
                void M(int argument)
                {
                    if (argument < 0) throw new ArgumentOutOfRangeException(nameof(argument), argument, "msg");
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenCombinedRangeOperandIsMemberAccess_ReportsDiagnosticAndFixes(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string source = """
            using System;

            class C
            {
                void M(string argument)
                {
                    if (argument.Length < 5 || argument.Length > 100) throw new ArgumentOutOfRangeException(nameof(argument.Length));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0003");

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            new ThrowIfOutOfRangeCodeFixProvider(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert
            .That(fixedSource)
            .Contains("ArgumentOutOfRangeException.ThrowIfOutOfRange(argument.Length, 5, 100);");
    }

    [Test]
    public async Task Analyze_WhenExceptionHasNoArguments_ReportsDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string source = """
            using System;

            class C
            {
                void M(int argument)
                {
                    if (argument < 0) throw new ArgumentOutOfRangeException();
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
    }

    [Test]
    [Arguments("bool argument", "argument")]
    [Arguments("int argument, int other", "argument < 5 || other > 100")]
    [Arguments("int argument", "argument > 100 || argument < 5")]
    [Arguments("int argument", "argument == 5 || argument > 100")]
    public async Task Analyze_WhenConditionIsUnrecognizedShape_DoesNotReportDiagnostic(
        string parameters,
        string condition,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

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

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenOperandIsEnumComparedForEquality_DoesNotReportDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string source = """
            using System;

            enum Kind
            {
                None,
            }

            class C
            {
                void M(Kind argument)
                {
                    if (argument == Kind.None) throw new ArgumentOutOfRangeException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenOperandIsObject_DoesNotReportDiagnostic(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string source = """
            using System;

            class C
            {
                void M(object argument, object other)
                {
                    if (argument == other) throw new ArgumentOutOfRangeException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenOperandTypeImplementsNeitherEquatableNorComparable_DoesNotReportDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string source = """
            using System;

            class UnsupportedType
            {
            }

            class C
            {
                void M(UnsupportedType argument, UnsupportedType other)
                {
                    if (argument == other) throw new ArgumentOutOfRangeException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenOperandTypeIsUnresolvedErrorType_DoesNotReportDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string source = """
            using System;

            class C
            {
                void M(Undeclared argument)
                {
                    if (argument < 0) throw new ArgumentOutOfRangeException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfOutOfRangeAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }
}
