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

        _ = await Assert.That(fixedSource).Contains("ObjectDisposedException.ThrowIf(_disposed, this);");
    }
}
