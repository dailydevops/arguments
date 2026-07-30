namespace NetEvolve.Arguments.Analyser.Tests.Unit;

using System;

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
    public async Task Analyze_WhenArgumentTypeIsReferenceType_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M(string argument)
                {
                    if (argument == default) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfDefaultAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenArgumentTypeIsNullableValueType_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M(Guid? argument)
                {
                    if (argument == default) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfDefaultAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenArgumentTypeIsStructWithoutIEquatable_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            struct S
            {
            }

            class C
            {
                void M(S argument)
                {
                    if (argument.Equals(default)) throw new ArgumentException(nameof(argument));
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

    [Test]
    public async Task CodeFix_WhenBlockContainsInteriorComment_PreservesCommentExactlyOnce()
    {
        const string source = """
            using System;

            class C
            {
                void M(Guid argument)
                {
                    // guard below
                    if (argument.Equals(default))
                    {
                        // see bug #431
                        throw new ArgumentException(nameof(argument));
                    }
                }
            }
            """;

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfDefaultAnalyzer(),
            new ThrowIfDefaultCodeFixProvider(),
            source
        );

        _ = await Assert.That(fixedSource).Contains("ArgumentException.ThrowIfDefault(argument);");

        // The comment that already precedes the `if` statement (carried over by WithTriviaFrom) must not be
        // duplicated, and the interior comment (attached to the `throw` inside the block, which WithTriviaFrom
        // alone would drop) must be preserved exactly once, immediately before the replacement statement.
        var guardOccurrences = CountOccurrences(fixedSource, "// guard below");
        var interiorOccurrences = CountOccurrences(fixedSource, "// see bug #431");

        _ = await Assert.That(guardOccurrences).IsEqualTo(1);
        _ = await Assert.That(interiorOccurrences).IsEqualTo(1);
        _ = await Assert
            .That(fixedSource.IndexOf("// guard below", StringComparison.Ordinal))
            .IsLessThan(fixedSource.IndexOf("// see bug #431", StringComparison.Ordinal));
        _ = await Assert
            .That(fixedSource)
            .Contains($"// see bug #431{Environment.NewLine}        ArgumentException.ThrowIfDefault");
    }

    private static int CountOccurrences(string text, string value) =>
        text.Split([value], StringSplitOptions.None).Length - 1;
}
