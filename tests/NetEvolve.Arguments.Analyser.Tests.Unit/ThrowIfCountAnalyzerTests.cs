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
    public async Task Analyze_WhenExceptionHasEmptyMessageAndMatchingParamName_ReportsDiagnostic()
    {
        const string source = """
            using System;
            using System.Collections.Generic;

            class C
            {
                void M(ICollection<int> argument)
                {
                    if (argument.Count > 100) throw new ArgumentException("", nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfCountAnalyzer(), source);

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
    }

    [Test]
    [Arguments("if (argument.Count > 100) throw new ArgumentNullException(nameof(argument));")]
    [Arguments("if (argument.Length > 100) throw new ArgumentException(nameof(argument));")]
    [Arguments("if (argument.Count == 100) throw new ArgumentException(nameof(argument));")]
    [Arguments("if (argument.Count > 100) throw new ArgumentException(nameof(other));")]
    [Arguments("if (argument.Count > 100) throw new ArgumentException(\"too many\", nameof(argument));")]
    [Arguments(
        """
            if (argument.Count > 100)
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
            using System.Collections.Generic;

            class C
            {
                void M(ICollection<int> argument, ICollection<int> other)
                {
                    {{statement}}
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfCountAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenReceiverIsUserDefinedNonCollectionType_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class Counter
            {
                public int Count { get; }
            }

            class C
            {
                void M(Counter argument)
                {
                    if (argument.Count > 100) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfCountAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenReceiverIsNonGenericCollection_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;
            using System.Collections;

            class C
            {
                void M(ArrayList argument)
                {
                    if (argument.Count > 100) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfCountAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenCountInvocationIsNotLinqEnumerableCount_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;
            using System.Collections;
            using System.Collections.Generic;

            class Counter : IEnumerable<int>
            {
                public int Count() => 0;

                public IEnumerator<int> GetEnumerator() => new List<int>().GetEnumerator();

                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            }

            class C
            {
                void M(Counter argument)
                {
                    if (argument.Count() > 100) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfCountAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenReceiverIsArray_ReportsDiagnostic()
    {
        const string source = """
            using System;
            using System.Linq;

            class C
            {
                void M(int[] argument)
                {
                    if (argument.Count() > 100) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfCountAnalyzer(), source);

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0007");
    }

    [Test]
    public async Task Analyze_WhenReceiverIsPlainIEnumerable_ReportsDiagnostic()
    {
        const string source = """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            class C
            {
                void M(IEnumerable<int> argument)
                {
                    if (argument.Count() > 100) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfCountAnalyzer(), source);

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0007");
    }

    [Test]
    public async Task Analyze_WhenReceiverIsString_ReportsDiagnostic()
    {
        const string source = """
            using System;
            using System.Linq;

            class C
            {
                void M(string argument)
                {
                    if (argument.Count() > 100) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfCountAnalyzer(), source);

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0007");
    }

    [Test]
    public async Task Analyze_WhenReceiverTypeIsUnresolved_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M()
                {
                    if (undeclaredArgument.Count > 100) throw new ArgumentException(nameof(undeclaredArgument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfCountAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }
}
