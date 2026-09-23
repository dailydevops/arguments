namespace NetEvolve.Arguments.Analyser.Tests.Unit;

using System.Threading;

public sealed class ThrowIfNullOrEmptyCollectionAnalyzerTests
{
    [Test]
    [Arguments("ICollection<int>", "argument is null || !argument.Any()")]
    [Arguments("ICollection<int>", "argument is null || argument.Count == 0")]
    [Arguments("ICollection<int>", "argument == null || 0 == argument.Count")]
    [Arguments("ICollection<int>", "argument is null || argument.Count() == 0")]
    [Arguments("ICollection<int>", "ReferenceEquals(argument, null) || argument.Count == 0")]
    [Arguments("ICollection<int>", "(argument is null) || (argument.Count == 0)")]
    [Arguments("IReadOnlyCollection<int>", "argument is null || argument.Count == 0")]
    [Arguments("IReadOnlyCollection<int>", "argument is null || !argument.Any()")]
    [Arguments("IList<int>", "argument is null || argument.Count == 0")]
    [Arguments("IReadOnlyList<int>", "argument is null || argument.Count == 0")]
    [Arguments("int[]", "argument is null || argument.Length == 0")]
    [Arguments("int[]", "argument is null || !argument.Any()")]
    [Arguments("int[]", "argument is null || argument.Count() == 0")]
    public async Task Analyze_WhenNullOrEmptyCollectionCheckThrowsArgumentException_ReportsDiagnosticAndFixes(
        string parameterType,
        string condition,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var source = $$"""
            using System;
            using System.Collections.Generic;
            using System.Linq;

            class C
            {
                void M({{parameterType}} argument)
                {
                    if ({{condition}}) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfNullOrEmptyCollectionAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0010");

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfNullOrEmptyCollectionAnalyzer(),
            new ThrowIfNullOrEmptyCollectionCodeFixProvider(),
            source,
            cancellationToken: cancellationToken
        );

        var expected = $$"""
            using System;
            using System.Collections.Generic;
            using System.Linq;

            class C
            {
                void M({{parameterType}} argument)
                {
                    ArgumentException.ThrowIfNullOrEmpty(argument);
                }
            }
            """;

        _ = await Assert.That(fixedSource).IsEqualTo(expected);
    }

    [Test]
    [Arguments("throw new ArgumentException(\"\", nameof(argument));")]
    [Arguments("throw new ArgumentException(\"argument\");")]
    [Arguments("throw new ArgumentException();")]
    [Arguments("throw new ArgumentNullException(nameof(argument));")]
    [Arguments("{ throw new ArgumentException(nameof(argument)); }")]
    public async Task Analyze_WhenThrownExceptionIsAccepted_ReportsDiagnostic(
        string body,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var source = $$"""
            using System;
            using System.Collections.Generic;

            class C
            {
                void M(ICollection<int> argument)
                {
                    if (argument is null || argument.Count == 0) {{body}}
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfNullOrEmptyCollectionAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
    }

    [Test]
    [Arguments(
        "ICollection<int>",
        "if (!argument.Any() || argument is null) throw new ArgumentException(nameof(argument));"
    )]
    [Arguments(
        "ICollection<int>",
        "if (argument.Count == 0 || argument is null) throw new ArgumentException(nameof(argument));"
    )]
    [Arguments("ICollection<int>", "if (argument.Count == 0) throw new ArgumentException(nameof(argument));")]
    [Arguments("ICollection<int>", "if (!argument.Any()) throw new ArgumentException(nameof(argument));")]
    [Arguments("ICollection<int>", "if (argument is null) throw new ArgumentException(nameof(argument));")]
    [Arguments(
        "ICollection<int>",
        "if (argument is not null || argument.Count == 0) throw new ArgumentException(nameof(argument));"
    )]
    [Arguments(
        "ICollection<int>",
        "if (argument is null && argument.Count == 0) throw new ArgumentException(nameof(argument));"
    )]
    [Arguments(
        "ICollection<int>",
        "if (other is null || argument.Count == 0) throw new ArgumentException(nameof(argument));"
    )]
    [Arguments(
        "ICollection<int>",
        "if (argument is null || other.Count == 0) throw new ArgumentException(nameof(argument));"
    )]
    [Arguments(
        "ICollection<int>",
        "if (argument is null || argument.Count == 1) throw new ArgumentException(nameof(argument));"
    )]
    [Arguments(
        "ICollection<int>",
        "if (argument is null || argument.Count != 0) throw new ArgumentException(nameof(argument));"
    )]
    [Arguments(
        "ICollection<int>",
        "if (argument is null || argument.Any()) throw new ArgumentException(nameof(argument));"
    )]
    [Arguments(
        "ICollection<int>",
        "if (argument is null || !argument.Any(x => x > 0)) throw new ArgumentException(nameof(argument));"
    )]
    [Arguments(
        "ICollection<int>",
        "if (argument is null || argument.Count == 0) throw new ArgumentException(nameof(other));"
    )]
    [Arguments(
        "ICollection<int>",
        "if (argument is null || argument.Count == 0) throw new ArgumentException(\"empty\", nameof(argument));"
    )]
    [Arguments(
        "ICollection<int>",
        "if (argument is null || argument.Count == 0) throw new InvalidOperationException(nameof(argument));"
    )]
    [Arguments(
        "ICollection<int>",
        "if (argument is null || argument.Count == 0) throw new ArgumentOutOfRangeException(nameof(argument));"
    )]
    [Arguments(
        "ICollection<int>",
        "if (argument is null || argument.Count == 0) { throw new ArgumentException(nameof(argument)); } else { }"
    )]
    [Arguments(
        "ICollection<int>",
        "if (argument is null || argument.Count == 0) { M(argument, other); throw new ArgumentException(nameof(argument)); }"
    )]
    [Arguments(
        "List<int>",
        "if (argument is null || argument.Count == 0) throw new ArgumentException(nameof(argument));"
    )]
    [Arguments("List<int>", "if (argument is null || !argument.Any()) throw new ArgumentException(nameof(argument));")]
    [Arguments(
        "HashSet<int>",
        "if (argument is null || argument.Count == 0) throw new ArgumentException(nameof(argument));"
    )]
    [Arguments(
        "Dictionary<int, int>",
        "if (argument is null || argument.Count == 0) throw new ArgumentException(nameof(argument));"
    )]
    [Arguments(
        "IEnumerable<int>",
        "if (argument is null || !argument.Any()) throw new ArgumentException(nameof(argument));"
    )]
    [Arguments(
        "IEnumerable<int>",
        "if (argument is null || argument.Count() == 0) throw new ArgumentException(nameof(argument));"
    )]
    [Arguments(
        "string",
        "if (argument is null || argument.Length == 0) throw new ArgumentException(nameof(argument));"
    )]
    [Arguments("string", "if (argument is null || !argument.Any()) throw new ArgumentException(nameof(argument));")]
    [Arguments("int[]", "if (argument is null || argument.Count == 0) throw new ArgumentException(nameof(argument));")]
    [Arguments(
        "int[,]",
        "if (argument is null || argument.Length == 0) throw new ArgumentException(nameof(argument));"
    )]
    [Arguments(
        "CustomCollection",
        "if (argument is null || argument.Count == 0) throw new ArgumentException(nameof(argument));"
    )]
    [Arguments(
        "CustomCollection",
        "if (argument is null || argument.Length == 0) throw new ArgumentException(nameof(argument));"
    )]
    [Arguments(
        "CustomCollection",
        "if (argument is null || !argument.Any()) throw new ArgumentException(nameof(argument));"
    )]
    public async Task Analyze_WhenConditionOrExceptionIsNotRecognized_DoesNotReportDiagnostic(
        string parameterType,
        string statement,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var source = $$"""
            using System;
            using System.Collections.Generic;
            using System.Linq;

            class CustomCollection
            {
                public int Count { get; }
                public int Length { get; }
                public bool Any() => Count > 0;
            }

            class C
            {
                void M({{parameterType}} argument, ICollection<int> other)
                {
                    {{statement}}
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfNullOrEmptyCollectionAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenArgumentIsGenericTypeParameter_DoesNotReportDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string source = """
            using System;
            using System.Collections.Generic;

            class C
            {
                void M<T>(T argument) where T : ICollection<int>
                {
                    if (argument is null || argument.Count == 0) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfNullOrEmptyCollectionAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenUsingModernReferences_StillReportsDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string source = """
            using System;
            using System.Collections.Generic;

            class C
            {
                void M(IReadOnlyList<int> argument)
                {
                    if (argument is null || argument.Count == 0) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfNullOrEmptyCollectionAnalyzer(),
            source,
            useLegacyReferences: false,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0010");
    }

    [Test]
    public async Task ApplyFix_WhenIfStatementHasBlockBodyAndComments_PreservesComments(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string source = """
            using System;
            using System.Collections.Generic;

            class C
            {
                void M(IList<int> argument)
                {
                    // Validate the argument.
                    if (argument is null || argument.Count == 0)
                    {
                        throw new ArgumentException(nameof(argument));
                    }
                }
            }
            """;

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfNullOrEmptyCollectionAnalyzer(),
            new ThrowIfNullOrEmptyCollectionCodeFixProvider(),
            source,
            cancellationToken: cancellationToken
        );

        const string expected = """
            using System;
            using System.Collections.Generic;

            class C
            {
                void M(IList<int> argument)
                {
                    // Validate the argument.
                    ArgumentException.ThrowIfNullOrEmpty(argument);
                }
            }
            """;

        _ = await Assert.That(fixedSource).IsEqualTo(expected);
    }

    [Test]
    public async Task ApplyFix_WhenSystemUsingIsMissing_AddsUsingDirective(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string source = """
            using System.Collections.Generic;

            class C
            {
                void M(ICollection<int> argument)
                {
                    if (argument is null || argument.Count == 0) throw new System.ArgumentException(nameof(argument));
                }
            }
            """;

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfNullOrEmptyCollectionAnalyzer(),
            new ThrowIfNullOrEmptyCollectionCodeFixProvider(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(fixedSource).Contains("using System;");
        _ = await Assert.That(fixedSource).Contains("ArgumentException.ThrowIfNullOrEmpty(argument);");
        _ = await Assert.That(fixedSource).DoesNotContain("throw new");
    }

    [Test]
    public async Task ApplyFixAll_WhenMultipleChecksInDocument_FixesAll(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string source = """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            class C
            {
                void M(ICollection<int> first, int[] second)
                {
                    if (first is null || !first.Any()) throw new ArgumentException(nameof(first));
                    if (second is null || second.Length == 0) throw new ArgumentException(nameof(second));
                }
            }
            """;

        var fixedSource = await AnalyzerVerifier.ApplyFixAllAsync(
            new ThrowIfNullOrEmptyCollectionAnalyzer(),
            new ThrowIfNullOrEmptyCollectionCodeFixProvider(),
            source,
            cancellationToken: cancellationToken
        );

        const string expected = """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            class C
            {
                void M(ICollection<int> first, int[] second)
                {
                    ArgumentException.ThrowIfNullOrEmpty(first);
                    ArgumentException.ThrowIfNullOrEmpty(second);
                }
            }
            """;

        _ = await Assert.That(fixedSource).IsEqualTo(expected);
    }
}
