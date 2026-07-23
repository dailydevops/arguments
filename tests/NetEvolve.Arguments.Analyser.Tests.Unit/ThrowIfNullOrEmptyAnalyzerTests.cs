namespace NetEvolve.Arguments.Analyser.Tests.Unit;

public sealed class ThrowIfNullOrEmptyAnalyzerTests
{
    [Test]
    public async Task Analyze_WhenIsNullOrEmptyCheckThrowsArgumentException_ReportsDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M(string? argument)
                {
                    if (string.IsNullOrEmpty(argument)) throw new ArgumentException("", nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfNullOrEmptyAnalyzer(), source);

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0002");
    }

    [Test]
    public async Task Analyze_WhenIsNullOrWhiteSpaceCheckThrowsArgumentException_ReportsDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M(string? argument)
                {
                    if (string.IsNullOrWhiteSpace(argument)) throw new ArgumentException("", nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfNullOrEmptyAnalyzer(), source);

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
    }

    [Test]
    public async Task Analyze_WhenThrowingArgumentNullException_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M(string? argument)
                {
                    if (string.IsNullOrEmpty(argument)) throw new ArgumentNullException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfNullOrEmptyAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenBuiltInThrowIfNullOrEmptyAvailable_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M(string? argument)
                {
                    if (string.IsNullOrEmpty(argument)) throw new ArgumentException("", nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfNullOrEmptyAnalyzer(),
            source,
            useLegacyReferences: false
        );

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
                    if (string.IsNullOrEmpty(argument))
                    {
                        throw new ArgumentException("", nameof(argument));
                    }
                    else
                    {
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfNullOrEmptyAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenMethodIsNotRecognized_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M(string? argument)
                {
                    if (string.IsInterned(argument) != null) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfNullOrEmptyAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenQualifierIsNotString_DoesNotReportDiagnostic()
    {
        const string source = """
            using System;

            class Other
            {
                public static bool IsNullOrEmpty(string? value) => value is null;
            }

            class C
            {
                void M(string? argument)
                {
                    if (Other.IsNullOrEmpty(argument)) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfNullOrEmptyAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenQualifiedAsSystemString_ReportsDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M(string? argument)
                {
                    if (String.IsNullOrEmpty(argument)) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfNullOrEmptyAnalyzer(), source);

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
    }

    [Test]
    public async Task CodeFix_WhenAppliedToIsNullOrEmpty_ReplacesWithThrowIfNullOrEmptyCall()
    {
        const string source = """
            using System;

            class C
            {
                void M(string? argument)
                {
                    if (string.IsNullOrEmpty(argument)) throw new ArgumentException("", nameof(argument));
                }
            }
            """;

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfNullOrEmptyAnalyzer(),
            new ThrowIfNullOrEmptyCodeFixProvider(),
            source
        );

        _ = await Assert.That(fixedSource).Contains("ArgumentException.ThrowIfNullOrEmpty(argument);");
    }

    [Test]
    public async Task CodeFix_WhenAppliedToIsNullOrWhiteSpace_ReplacesWithThrowIfNullOrWhiteSpaceCall()
    {
        const string source = """
            using System;

            class C
            {
                void M(string? argument)
                {
                    if (string.IsNullOrWhiteSpace(argument)) throw new ArgumentException("", nameof(argument));
                }
            }
            """;

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfNullOrEmptyAnalyzer(),
            new ThrowIfNullOrEmptyCodeFixProvider(),
            source
        );

        _ = await Assert.That(fixedSource).Contains("ArgumentException.ThrowIfNullOrWhiteSpace(argument);");
    }
}
