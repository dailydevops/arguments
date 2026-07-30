namespace NetEvolve.Arguments.Analyser.Tests.Unit;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

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
    public async Task Analyze_WhenOnlyThrowIfNullOrEmptyIsBuiltIn_StillReportsDiagnosticForIsNullOrWhiteSpace()
    {
        // Simulates net7.0's System.Runtime shape, where ArgumentException.ThrowIfNullOrEmpty already exists
        // but ArgumentException.ThrowIfNullOrWhiteSpace does not (it arrived only in .NET 8). Gating the whole
        // rule on a single probe for "ThrowIfNullOrEmpty" would wrongly suppress this IsNullOrWhiteSpace case,
        // even though NetEvolve.Arguments's own polyfill supplies that helper for exactly that framework.
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

        var diagnostics = await GetDiagnosticsWithOnlyThrowIfNullOrEmptyBuiltInAsync(source);

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0002");
    }

    [Test]
    public async Task Analyze_WhenOnlyThrowIfNullOrEmptyIsBuiltIn_DoesNotReportDiagnosticForIsNullOrEmpty()
    {
        // Same simulated net7.0 shape as above, but for the branch whose built-in helper already exists:
        // that branch must stay suppressed to avoid duplicating the built-in CA1511 analyzer.
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

        var diagnostics = await GetDiagnosticsWithOnlyThrowIfNullOrEmptyBuiltInAsync(source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    /// <summary>
    /// Compiles <paramref name="source"/> against a minimal, hand-written substitute BCL whose <c>System.ArgumentException</c>
    /// declares only <c>ThrowIfNullOrEmpty</c> — reproducing net7.0's System.Runtime shape, which neither of
    /// <see cref="AnalyzerVerifier"/>'s two reference sets (all built-ins present, or none) can represent.
    /// </summary>
    /// <param name="source">The C# source to analyze.</param>
    /// <returns>The diagnostics <see cref="ThrowIfNullOrEmptyAnalyzer"/> reports for <paramref name="source"/>.</returns>
    private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsWithOnlyThrowIfNullOrEmptyBuiltInAsync(
        string source
    )
    {
        const string stubBclSource = """
            namespace System
            {
                public class Object
                {
                    public virtual bool Equals(object? obj) => false;
                    public virtual int GetHashCode() => 0;
                    public virtual string? ToString() => null;
                }

                public class Void { }

                public struct Boolean { }

                public struct Int32 { }

                public struct Char { }

                public class String
                {
                    public static bool IsNullOrEmpty(string? value) => false;

                    public static bool IsNullOrWhiteSpace(string? value) => false;
                }

                public class ValueType { }

                public struct Nullable<T>
                    where T : struct { }

                public class Attribute { }

                public class Exception
                {
                    public Exception() { }

                    public Exception(string message) { }
                }

                // Only ThrowIfNullOrEmpty is declared here, matching net7.0's System.Runtime: it introduced
                // ArgumentException.ThrowIfNullOrEmpty, while ThrowIfNullOrWhiteSpace arrived only in .NET 8.
                public class ArgumentException : Exception
                {
                    public ArgumentException() { }

                    public ArgumentException(string message) : base(message) { }

                    public ArgumentException(string message, string paramName) : base(message) { }

                    public static void ThrowIfNullOrEmpty(string? argument, string? paramName = null) { }
                }
            }

            namespace System.Runtime.CompilerServices
            {
                public class RuntimeCompatibilityAttribute : System.Attribute
                {
                    public bool WrapNonExceptionThrows { get; set; }
                }
            }
            """;

        var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[]
            {
                CSharpSyntaxTree.ParseText(stubBclSource, parseOptions),
                CSharpSyntaxTree.ParseText(source, parseOptions),
            },
            Array.Empty<MetadataReference>(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        // Guard against the stub BCL silently failing to bind (e.g. a missing well-known type), which would
        // make the "no diagnostic" assertions in the calling tests pass for the wrong reason.
        var compilationErrors = compilation
            .GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToImmutableArray();

        if (!compilationErrors.IsEmpty)
        {
            throw new InvalidOperationException(
                $"Stub BCL compilation failed: {string.Join(Environment.NewLine, compilationErrors)}"
            );
        }

        var compilationWithAnalyzers = compilation.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(new ThrowIfNullOrEmptyAnalyzer())
        );

        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(CancellationToken.None).ConfigureAwait(false);
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
