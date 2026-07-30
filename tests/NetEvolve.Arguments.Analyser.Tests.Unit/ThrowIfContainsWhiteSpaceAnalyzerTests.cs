namespace NetEvolve.Arguments.Analyser.Tests.Unit;

public sealed class ThrowIfContainsWhiteSpaceAnalyzerTests
{
    [Test]
    [Arguments("argument.Any(c => char.IsWhiteSpace(c))")]
    [Arguments("argument.Any(char.IsWhiteSpace)")]
    public async Task Analyze_WhenWhiteSpaceCheckThrowsArgumentException_ReportsDiagnosticAndFixes(string condition)
    {
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

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfContainsWhiteSpaceAnalyzer(), source);

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0008");

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfContainsWhiteSpaceAnalyzer(),
            new ThrowIfContainsWhiteSpaceCodeFixProvider(),
            source
        );

        _ = await Assert.That(fixedSource).Contains("ArgumentException.ThrowIfContainsWhiteSpace(argument);");
    }

    [Test]
    [Arguments("if (argument.Any(char.IsWhiteSpace)) throw new ArgumentNullException(nameof(argument));")]
    [Arguments("if (argument.Any(c => c == ' ')) throw new ArgumentException(nameof(argument));")]
    [Arguments("if (argument.Any(c => char.IsWhiteSpace(other))) throw new ArgumentException(nameof(argument));")]
    [Arguments("if (argument.Contains(' ')) throw new ArgumentException(nameof(argument));")]
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
    public async Task Analyze_WhenConditionOrExceptionIsNotRecognized_DoesNotReportDiagnostic(string statement)
    {
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

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfContainsWhiteSpaceAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    [Arguments("char[]")]
    [Arguments("System.Collections.Generic.List<char>")]
    [Arguments("System.Collections.Generic.IEnumerable<char>")]
    public async Task Analyze_WhenReceiverIsNotString_DoesNotReportDiagnostic(string parameterType)
    {
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

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfContainsWhiteSpaceAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenReceiverHasNoResolvableType_DoesNotReportDiagnostic()
    {
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

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfContainsWhiteSpaceAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenAnyResolutionIsAmbiguous_DoesNotReportDiagnostic()
    {
        // Two equally applicable extension methods named "Any" make the call ambiguous, so
        // SemanticModel.GetSymbolInfo(...).Symbol is null (CandidateReason.OverloadResolutionFailure).
        // This exercises the "Symbol is IMethodSymbol" pattern failing in IsLinqEnumerableAny, as
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

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfContainsWhiteSpaceAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenAnyIsNotLinqEnumerableAny_DoesNotReportDiagnostic()
    {
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

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(new ThrowIfContainsWhiteSpaceAnalyzer(), source);

        _ = await Assert.That(diagnostics).IsEmpty();
    }
}
