namespace NetEvolve.Arguments.Analyser.Tests.Unit;

using System;

public sealed class ThrowIfNullAnalyzerTests
{
    [Test]
    public async Task Analyze_WhenIsNullCheckThrowsArgumentNullException_ReportsDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M(string? argument)
                {
                    if (argument is null) throw new ArgumentNullException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfNullAnalyzer(), source);

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0001");
    }

    [Test]
    [Arguments("argument == null")]
    [Arguments("null == argument")]
    [Arguments("argument is null")]
    [Arguments("ReferenceEquals(argument, null)")]
    [Arguments("ReferenceEquals(null, argument)")]
    [Arguments("!(argument != null)")]
    [Arguments("!(argument is not null)")]
    [Arguments("!(null != argument)")]
    [Arguments("object.ReferenceEquals(argument, null)")]
    [Arguments("(argument is null)")]
    public async Task Analyze_WhenUsingRecognizedNullCheckVariant_ReportsDiagnostic(string condition)
    {
        var source = $$"""
            using System;

            class C
            {
                void M(string? argument)
                {
                    if ({{condition}}) throw new ArgumentNullException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfNullAnalyzer(), source);

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
    }

    [Test]
    [Arguments("argument is not null")]
    [Arguments("!(argument is null)")]
    [Arguments("argument != null")]
    [Arguments("null != argument")]
    public async Task Analyze_WhenConditionMeansNonNull_DoesNotReportDiagnostic(string condition)
    {
        var source = $$"""
            using System;

            class C
            {
                void M(string? argument)
                {
                    if ({{condition}}) throw new ArgumentNullException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfNullAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenCoalesceThrowsArgumentNullException_ReportsDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                private readonly string _value;

                public C(string? argument)
                {
                    _value = argument ?? throw new ArgumentNullException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfNullAnalyzer(), source);

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0001");
    }

    [Test]
    public async Task CodeFix_WhenAppliedToCoalesce_HoistsThrowIfNullAndKeepsAssignment()
    {
        const string source = """
            using System;

            class C
            {
                private readonly string _value;

                public C(string? argument)
                {
                    _value = argument ?? throw new ArgumentNullException(nameof(argument));
                }
            }
            """;

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfNullAnalyzer(),
            new ThrowIfNullCodeFixProvider(),
            source
        );

        _ = await Assert.That(fixedSource).Contains("ArgumentNullException.ThrowIfNull(argument);");
        _ = await Assert.That(fixedSource).Contains("_value = argument;");
        _ = await Assert.That(fixedSource).DoesNotContain("throw new ArgumentNullException");
    }

    [Test]
    public async Task CodeFix_WhenAppliedToCoalesceWithRegionTrivia_DoesNotDuplicateLeadingTrivia()
    {
        const string source = """
            using System;

            class C
            {
                private readonly string _value;

                public C(string? argument)
                {
                    #region Guards
                    _value = argument ?? throw new ArgumentNullException(nameof(argument));
                    #endregion
                }
            }
            """;

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfNullAnalyzer(),
            new ThrowIfNullCodeFixProvider(),
            source
        );

        var regionOccurrences = CountOccurrences(fixedSource, "#region Guards");
        var endRegionOccurrences = CountOccurrences(fixedSource, "#endregion");

        _ = await Assert.That(regionOccurrences).IsEqualTo(1);
        _ = await Assert.That(endRegionOccurrences).IsEqualTo(1);
        _ = await Assert.That(fixedSource).Contains("ArgumentNullException.ThrowIfNull(argument);");
        _ = await Assert.That(fixedSource).Contains("_value = argument;");
    }

    [Test]
    public async Task CodeFix_WhenAppliedToCoalesceWithLeadingComment_DoesNotDuplicateComment()
    {
        const string source = """
            using System;

            class C
            {
                private readonly string _value;

                public C(string? argument)
                {
                    // validate input
                    _value = argument ?? throw new ArgumentNullException(nameof(argument));
                }
            }
            """;

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfNullAnalyzer(),
            new ThrowIfNullCodeFixProvider(),
            source
        );

        var commentOccurrences = CountOccurrences(fixedSource, "// validate input");

        _ = await Assert.That(commentOccurrences).IsEqualTo(1);
        _ = await Assert.That(fixedSource).Contains("ArgumentNullException.ThrowIfNull(argument);");
        _ = await Assert.That(fixedSource).Contains("_value = argument;");
    }

    private static int CountOccurrences(string source, string value)
    {
        var count = 0;
        var index = 0;

        while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }

    [Test]
    public async Task Analyze_WhenBuiltInThrowIfNullAvailable_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M(string? argument)
                {
                    if (argument is null) throw new ArgumentNullException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfNullAnalyzer(),
            source,
            useLegacyReferences: false
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenExceptionHasNoArguments_ReportsDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M(string? argument)
                {
                    if (argument is null) throw new ArgumentNullException();
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfNullAnalyzer(), source);

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
    }

    [Test]
    public async Task Analyze_WhenParamNameDoesNotMatchCheckedArgument_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M(string? argument, string other)
                {
                    if (argument is null) throw new ArgumentNullException(nameof(other));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfNullAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    [Arguments("_value = argument ?? throw new ArgumentException(nameof(argument));")]
    [Arguments("_value = argument ?? throw new ArgumentNullException(nameof(argument), \"custom\");")]
    public async Task Analyze_WhenCoalesceThrowIsNotRecognized_DoesNotReportDiagnostic(string statement)
    {
        var source = $$"""
            using System;

            class C
            {
                private readonly string _value;

                public C(string? argument)
                {
                    {{statement}}
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfNullAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenExceptionHasCustomMessage_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M(string? argument)
                {
                    if (argument is null) throw new ArgumentNullException(nameof(argument), "custom message");
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfNullAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenIfHasElseClause_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M(string? argument)
                {
                    if (argument is null)
                    {
                        throw new ArgumentNullException(nameof(argument));
                    }
                    else
                    {
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfNullAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenCoalesceInSimpleLambdaParameterExpression_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M()
                {
                    Func<string, string> f = s => s ?? throw new ArgumentNullException(nameof(s));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfNullAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenCoalesceInParenthesizedLambdaCapturingOuterVariable_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M(string? a)
                {
                    Func<string> f = () => a ?? throw new ArgumentNullException(nameof(a));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfNullAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenCoalesceInsideConditionalExpressionBranch_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M(bool flag, string? a)
                {
                    var x = flag ? (a ?? throw new ArgumentNullException(nameof(a))) : "d";
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfNullAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenCoalesceInStatementBodiedLambda_ReportsDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M()
                {
                    Action<string> f = s =>
                    {
                        _ = s ?? throw new ArgumentNullException(nameof(s));
                    };
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfNullAnalyzer(), source);

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0001");
    }

    [Test]
    public async Task CodeFix_WhenCoalesceIsNestedInsideIfStatement_HoistsThrowIfNullAndKeepsAssignment()
    {
        const string source = """
            using System;

            class C
            {
                void M(string? target, string? source)
                {
                    if (target is null)
                    {
                        target = source ?? throw new ArgumentNullException(nameof(source));
                    }
                }
            }
            """;

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfNullAnalyzer(),
            new ThrowIfNullCodeFixProvider(),
            source
        );

        _ = await Assert.That(fixedSource).Contains("ArgumentNullException.ThrowIfNull(source);");
        _ = await Assert.That(fixedSource).Contains("target = source;");
        _ = await Assert.That(fixedSource).DoesNotContain("throw new ArgumentNullException");
    }

    [Test]
    public async Task CodeFix_WhenCoalesceIsNestedInsideUnrelatedIfStatement_StillAppliesCoalesceFix()
    {
        const string source = """
            using System;

            class C
            {
                void M(bool flag, string? source)
                {
                    if (flag)
                    {
                        var value = source ?? throw new ArgumentNullException(nameof(source));
                    }
                }
            }
            """;

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfNullAnalyzer(),
            new ThrowIfNullCodeFixProvider(),
            source
        );

        _ = await Assert.That(fixedSource).Contains("ArgumentNullException.ThrowIfNull(source);");
        _ = await Assert.That(fixedSource).Contains("var value = source;");
        _ = await Assert.That(fixedSource).DoesNotContain("throw new ArgumentNullException");
    }

    [Test]
    public async Task CodeFix_WhenApplied_ReplacesWithThrowIfNullCall()
    {
        const string source = """
            using System;

            class C
            {
                void M(string? argument)
                {
                    if (argument is null) throw new ArgumentNullException(nameof(argument));
                }
            }
            """;

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfNullAnalyzer(),
            new ThrowIfNullCodeFixProvider(),
            source
        );

        _ = await Assert.That(fixedSource).Contains("ArgumentNullException.ThrowIfNull(argument);");
        _ = await Assert.That(fixedSource).DoesNotContain("throw new ArgumentNullException");
    }

    [Test]
    public async Task CodeFix_WhenSourceHasNoUsingSystemAndThrowIsFullyQualified_AddsUsingSystemDirective()
    {
        const string source = """
            class C
            {
                void M(string? argument)
                {
                    if (argument is null) throw new System.ArgumentNullException(nameof(argument));
                }
            }
            """;

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfNullAnalyzer(),
            new ThrowIfNullCodeFixProvider(),
            source
        );

        _ = await Assert.That(fixedSource).Contains("using System;");
        _ = await Assert.That(fixedSource).Contains("ArgumentNullException.ThrowIfNull(argument);");
        _ = await Assert.That(fixedSource).DoesNotContain("throw new");
    }

    [Test]
    public async Task CodeFix_WhenFileScopedNamespaceHasNoUsingSystem_AddsUsingSystemDirective()
    {
        const string source = """
            namespace Test;

            class C
            {
                void M(string? argument)
                {
                    if (argument is null) throw new System.ArgumentNullException(nameof(argument));
                }
            }
            """;

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfNullAnalyzer(),
            new ThrowIfNullCodeFixProvider(),
            source
        );

        _ = await Assert.That(fixedSource).Contains("using System;");
        _ = await Assert.That(fixedSource).Contains("ArgumentNullException.ThrowIfNull(argument);");
        _ = await Assert.That(fixedSource.Split("using System;").Length).IsEqualTo(2);
    }

    [Test]
    public async Task CodeFix_WhenFileScopedNamespaceAlreadyHasUsingSystem_DoesNotDuplicateDirective()
    {
        const string source = """
            namespace Test;

            using System;

            class C
            {
                void M(string? argument)
                {
                    if (argument is null) throw new System.ArgumentNullException(nameof(argument));
                }
            }
            """;

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfNullAnalyzer(),
            new ThrowIfNullCodeFixProvider(),
            source
        );

        _ = await Assert.That(fixedSource).Contains("ArgumentNullException.ThrowIfNull(argument);");
        _ = await Assert.That(fixedSource.Split("using System;").Length).IsEqualTo(2);
    }

    [Test]
    public async Task CodeFix_WhenBlockScopedNamespaceHasNoUsingSystem_AddsUsingSystemDirective()
    {
        const string source = """
            namespace Test
            {
                class C
                {
                    void M(string? argument)
                    {
                        if (argument is null) throw new System.ArgumentNullException(nameof(argument));
                    }
                }
            }
            """;

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfNullAnalyzer(),
            new ThrowIfNullCodeFixProvider(),
            source
        );

        _ = await Assert.That(fixedSource).Contains("using System;");
        _ = await Assert.That(fixedSource).Contains("ArgumentNullException.ThrowIfNull(argument);");
        _ = await Assert.That(fixedSource.Split("using System;").Length).IsEqualTo(2);
    }

    [Test]
    public async Task CodeFix_WhenBlockScopedNamespaceAlreadyHasUsingSystem_DoesNotDuplicateDirective()
    {
        const string source = """
            namespace Test
            {
                using System;

                class C
                {
                    void M(string? argument)
                    {
                        if (argument is null) throw new System.ArgumentNullException(nameof(argument));
                    }
                }
            }
            """;

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfNullAnalyzer(),
            new ThrowIfNullCodeFixProvider(),
            source
        );

        _ = await Assert.That(fixedSource).Contains("ArgumentNullException.ThrowIfNull(argument);");
        _ = await Assert.That(fixedSource.Split("using System;").Length).IsEqualTo(2);
    }

    [Test]
    public async Task CodeFix_WhenSourceHasOnlyAliasedSystemUsing_AddsRealUsingSystemDirective()
    {
        const string source = """
            using SysArg = System;

            class C
            {
                void M(string? argument)
                {
                    if (argument is null) throw new System.ArgumentNullException(nameof(argument));
                }
            }
            """;

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfNullAnalyzer(),
            new ThrowIfNullCodeFixProvider(),
            source
        );

        _ = await Assert.That(fixedSource).Contains("using SysArg = System;");
        _ = await Assert.That(fixedSource).Contains("using System;");
        _ = await Assert.That(fixedSource).Contains("ArgumentNullException.ThrowIfNull(argument);");
    }
}
