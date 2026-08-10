namespace NetEvolve.Arguments.Analyser.Tests.Unit;

using System.Threading;

public sealed class ThrowIfContainsWhiteSpaceAnalyzerTests
{
    [Test]
    [Arguments("argument.Any(c => char.IsWhiteSpace(c))")]
    [Arguments("argument.Any(char.IsWhiteSpace)")]
    [Arguments("argument.Count(c => char.IsWhiteSpace(c)) > 0")]
    [Arguments("argument.Count(char.IsWhiteSpace) > 0")]
    [Arguments("argument.Count(c => char.IsWhiteSpace(c)) >= 1")]
    [Arguments("argument.Count(char.IsWhiteSpace) >= 1")]
    [Arguments("argument.Count(c => char.IsWhiteSpace(c)) != 0")]
    [Arguments("argument.Count(char.IsWhiteSpace) != 0")]
    [Arguments("0 < argument.Count(char.IsWhiteSpace)")]
    [Arguments("1 <= argument.Count(char.IsWhiteSpace)")]
    [Arguments("0 != argument.Count(char.IsWhiteSpace)")]
    [Arguments("argument.Where(c => char.IsWhiteSpace(c)).Any()")]
    [Arguments("argument.Where(char.IsWhiteSpace).Any()")]
    public async Task Analyze_WhenWhiteSpaceCheckThrowsArgumentException_ReportsDiagnosticAndFixes(
        string condition,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var source = $$"""
            using System;
            using System.Linq;

            class C
            {
                void M(string argument)
                {
                    if ({{condition}}) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfContainsWhiteSpaceAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0008");

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfContainsWhiteSpaceAnalyzer(),
            new ThrowIfContainsWhiteSpaceCodeFixProvider(),
            source,
            cancellationToken: cancellationToken
        );

        const string expected = """
            using System;
            using System.Linq;

            class C
            {
                void M(string argument)
                {
                    ArgumentException.ThrowIfContainsWhiteSpace(argument);
                }
            }
            """;

        _ = await Assert.That(fixedSource).IsEqualTo(expected);
    }

    [Test]
    public async Task Analyze_WhenExceptionHasEmptyMessageAndMatchingParamName_ReportsDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string source = """
            using System;
            using System.Linq;

            class C
            {
                void M(string argument)
                {
                    if (argument.Any(char.IsWhiteSpace)) throw new ArgumentException("", nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfContainsWhiteSpaceAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
    }

    [Test]
    [Arguments("if (argument.Any(char.IsWhiteSpace)) throw new ArgumentNullException(nameof(argument));")]
    [Arguments("if (argument.Any(c => c == ' ')) throw new ArgumentException(nameof(argument));")]
    [Arguments("if (argument.Any(c => char.IsWhiteSpace(other))) throw new ArgumentException(nameof(argument));")]
    [Arguments("if (argument.Contains(' ')) throw new ArgumentException(nameof(argument));")]
    [Arguments("if (argument.Any(char.IsWhiteSpace)) throw new ArgumentException(nameof(other));")]
    [Arguments(
        "if (argument.Any(char.IsWhiteSpace)) throw new ArgumentException(\"has whitespace\", nameof(argument));"
    )]
    [Arguments("if (argument.Count(char.IsWhiteSpace) > 0) throw new ArgumentException(nameof(other));")]
    [Arguments(
        "if (argument.Count(char.IsWhiteSpace) > 0) throw new ArgumentException(\"has whitespace\", nameof(argument));"
    )]
    [Arguments("if (argument.Where(char.IsWhiteSpace).Any()) throw new ArgumentException(nameof(other));")]
    [Arguments(
        "if (argument.Where(char.IsWhiteSpace).Any()) throw new ArgumentException(\"has whitespace\", nameof(argument));"
    )]
    [Arguments("if (argument.Count(char.IsWhiteSpace) > 1) throw new ArgumentException(nameof(argument));")]
    [Arguments("if (argument.Count(char.IsWhiteSpace) == 0) throw new ArgumentException(nameof(argument));")]
    [Arguments("if (argument.Count(char.IsWhiteSpace) < 1) throw new ArgumentException(nameof(argument));")]
    [Arguments("if (1 < argument.Count(char.IsWhiteSpace)) throw new ArgumentException(nameof(argument));")]
    [Arguments("if (argument.All(char.IsWhiteSpace)) throw new ArgumentException(nameof(argument));")]
    [Arguments("if (argument.Count(c => c == ' ') > 0) throw new ArgumentException(nameof(argument));")]
    [Arguments("if (argument.Select(c => c).Any()) throw new ArgumentException(nameof(argument));")]
    [Arguments("if (argument.Where(c => c == ' ').Any()) throw new ArgumentException(nameof(argument));")]
    [Arguments(
        """
            if (argument.Any(char.IsWhiteSpace))
            {
                throw new ArgumentException(nameof(argument));
            }
            else
            {
            }
            """
    )]
    public async Task Analyze_WhenConditionOrExceptionIsNotRecognized_DoesNotReportDiagnostic(
        string statement,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var source = $$"""
            using System;
            using System.Linq;

            class C
            {
                void M(string argument, char other)
                {
                    {{statement}}
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfContainsWhiteSpaceAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    [Arguments("char[]")]
    [Arguments("System.Collections.Generic.List<char>")]
    [Arguments("System.Collections.Generic.IEnumerable<char>")]
    public async Task Analyze_WhenReceiverIsNotString_DoesNotReportDiagnostic(
        string parameterType,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var source = $$"""
            using System;
            using System.Linq;

            class C
            {
                void M({{parameterType}} chars)
                {
                    if (chars.Any(char.IsWhiteSpace)) throw new ArgumentException(nameof(chars));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfContainsWhiteSpaceAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    [Arguments("char[]", "chars.Count(char.IsWhiteSpace) > 0")]
    [Arguments("System.Collections.Generic.List<char>", "chars.Count(char.IsWhiteSpace) > 0")]
    [Arguments("char[]", "chars.Where(char.IsWhiteSpace).Any()")]
    [Arguments("System.Collections.Generic.List<char>", "chars.Where(char.IsWhiteSpace).Any()")]
    public async Task Analyze_WhenReceiverIsNotString_NewShapes_DoesNotReportDiagnostic(
        string parameterType,
        string condition,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var source = $$"""
            using System;
            using System.Linq;

            class C
            {
                void M({{parameterType}} chars)
                {
                    if ({{condition}}) throw new ArgumentException(nameof(chars));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfContainsWhiteSpaceAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenReceiverHasNoResolvableType_DoesNotReportDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        // "Predicate" refers to the method group C.Predicate(char), which has no type of its own
        // (SemanticModel.GetTypeInfo(...).Type is null for it), exercising the null-propagation
        // branch of IsStringReceiver rather than the "resolved but not string" branch already
        // covered by Analyze_WhenReceiverIsNotString_DoesNotReportDiagnostic.
        var source = """
            using System;
            using System.Linq;

            class C
            {
                static bool Predicate(char c) => char.IsWhiteSpace(c);

                void M()
                {
                    if (Predicate.Any(char.IsWhiteSpace)) throw new ArgumentException("argument");
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfContainsWhiteSpaceAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    [Arguments("argument.Count(char.IsWhiteSpace) > 0")]
    [Arguments("argument.Where(char.IsWhiteSpace).Any()")]
    public async Task Analyze_WhenCountOrWhereIsUserDefinedExtension_DoesNotReportDiagnostic(
        string condition,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var source = $$"""
            using System;

            static class MyExtensions
            {
                public static int Count(this string s, Func<char, bool> predicate) => 0;

                public static string[] Where(this string s, Func<char, bool> predicate) => Array.Empty<string>();

                public static bool Any(this string[] s) => false;
            }

            class C
            {
                void M(string argument)
                {
                    if ({{condition}}) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfContainsWhiteSpaceAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenAnyIsUserDefinedExtensionOnWhereResult_DoesNotReportDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var source = """
            using System;
            using System.Linq;

            static class MyExtensions
            {
                public static bool Any(this System.Collections.Generic.IEnumerable<char> s) => false;
            }

            class C
            {
                void M(string argument)
                {
                    if (argument.Where(char.IsWhiteSpace).Any()) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfContainsWhiteSpaceAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenAnyResolutionIsAmbiguous_DoesNotReportDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Two equally applicable extension methods named "Any" make the call ambiguous, so
        // SemanticModel.GetSymbolInfo(...).Symbol is null (CandidateReason.OverloadResolutionFailure).
        // This exercises the "Symbol is IMethodSymbol" pattern failing in IsLinqEnumerableMethod, as
        // opposed to Analyze_WhenAnyIsNotLinqEnumerableAny_DoesNotReportDiagnostic, where the symbol
        // resolves unambiguously to a method whose containing type merely isn't Enumerable.
        var source = """
            using System;
            using System.Linq;

            static class CustomExtensionsA
            {
                public static bool Any(this string value, Func<char, bool> predicate) => false;
            }

            static class CustomExtensionsB
            {
                public static bool Any(this string value, Func<char, bool> predicate) => false;
            }

            class C
            {
                void M(string argument)
                {
                    if (argument.Any(char.IsWhiteSpace)) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfContainsWhiteSpaceAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenAnyIsNotLinqEnumerableAny_DoesNotReportDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var source = """
            using System;
            using System.Linq;

            static class CustomExtensions
            {
                public static bool Any(this string value, Func<char, bool> predicate) => false;
            }

            class C
            {
                void M(string argument)
                {
                    if (argument.Any(char.IsWhiteSpace)) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfContainsWhiteSpaceAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }
}
