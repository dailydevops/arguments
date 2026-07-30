namespace NetEvolve.Arguments.Analyser.Tests.Unit;

public sealed class ThrowIfDisposedAnalyzerTests
{
    [Test]
    public async Task Analyze_WhenDisposedCheckThrowsObjectDisposedException_ReportsDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                private bool _disposed;

                void M()
                {
                    if (_disposed) throw new ObjectDisposedException(GetType().Name);
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfDisposedAnalyzer(), source);

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0005");
    }

    [Test]
    public async Task Analyze_WhenExceptionHasAnExplicitMessage_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                private bool _disposed;

                void M()
                {
                    if (_disposed) throw new ObjectDisposedException(nameof(C), "conn closed");
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfDisposedAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenInStaticMethod_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                private static bool _disposed;

                static void M()
                {
                    if (_disposed) throw new ObjectDisposedException(nameof(C));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfDisposedAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenInLocalFunctionInsideStaticMethod_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                private static bool _disposed;

                static void M()
                {
                    void Check()
                    {
                        if (_disposed) throw new ObjectDisposedException(nameof(C));
                    }

                    Check();
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfDisposedAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenInLambdaInsideStaticMethod_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                private static bool _disposed;

                static void M()
                {
                    Action check = () =>
                    {
                        if (_disposed) throw new ObjectDisposedException(nameof(C));
                    };

                    check();
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfDisposedAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenInLocalFunctionInsideInstanceMethod_ReportsDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                private bool _disposed;

                void M()
                {
                    void Check()
                    {
                        if (_disposed) throw new ObjectDisposedException(nameof(C));
                    }

                    Check();
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfDisposedAnalyzer(), source);

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0005");
    }

    [Test]
    [Arguments("if (_disposed) throw new ArgumentException(\"disposed\");")]
    [Arguments(
        """
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            else
            {
            }
            """
    )]
    public async Task Analyze_WhenExceptionTypeOrShapeIsNotRecognized_DoesNotReportDiagnostic(string statement)
    {
        var source = $$"""
            using System;

            class C
            {
                private bool _disposed;

                void M()
                {
                    {{statement}}
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfDisposedAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenBuiltInThrowIfAvailable_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                private bool _disposed;

                void M()
                {
                    if (_disposed) throw new ObjectDisposedException(GetType().Name);
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfDisposedAnalyzer(),
            source,
            useLegacyReferences: false
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task CodeFix_WhenApplied_ReplacesWithThrowIfCall()
    {
        const string source = """
            using System;

            class C
            {
                private bool _disposed;

                void M()
                {
                    if (_disposed) throw new ObjectDisposedException(GetType().Name);
                }
            }
            """;

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfDisposedAnalyzer(),
            new ThrowIfDisposedCodeFixProvider(),
            source
        );

        const string expected = """
            using System;

            class C
            {
                private bool _disposed;

                void M()
                {
                    ObjectDisposedException.ThrowIf(_disposed, this);
                }
            }
            """;

        _ = await Assert.That(fixedSource).IsEqualTo(expected);
    }
}
