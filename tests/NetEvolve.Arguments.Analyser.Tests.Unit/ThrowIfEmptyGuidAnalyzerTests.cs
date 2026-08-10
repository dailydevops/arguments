namespace NetEvolve.Arguments.Analyser.Tests.Unit;

using System.Threading;

public sealed class ThrowIfEmptyGuidAnalyzerTests
{
    [Test]
    [Arguments("argument == Guid.Empty")]
    [Arguments("Guid.Empty == argument")]
    [Arguments("argument.Equals(Guid.Empty)")]
    public async Task Analyze_WhenEmptyGuidCheckThrowsArgumentException_ReportsDiagnostic(
        string condition,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

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

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfEmptyGuidAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).Count().IsEqualTo(1);
        _ = await Assert.That(diagnostics[0].Id).IsEqualTo("NEA0009");
    }

    [Test]
    [Arguments("if (argument == Guid.Empty) throw new ArgumentNullException(nameof(argument));")]
    [Arguments("if (argument == Guid.Empty) throw new ArgumentException(\"custom\", nameof(argument));")]
    [Arguments("if (argument == Guid.NewGuid()) throw new ArgumentException(nameof(argument));")]
    [Arguments(
        """
            if (argument == Guid.Empty)
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

            class C
            {
                void M(Guid argument)
                {
                    {{statement}}
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfEmptyGuidAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenArgumentIsNullableGuid_DoesNotReportDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string source = """
            using System;

            class C
            {
                void M(Guid? argument)
                {
                    if (argument == Guid.Empty) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfEmptyGuidAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenArgumentIsObject_DoesNotReportDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string source = """
            using System;

            class C
            {
                void M(object argument)
                {
                    if (argument.Equals(Guid.Empty)) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfEmptyGuidAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task Analyze_WhenGuidIsUserDefinedType_DoesNotReportDiagnostic(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string source = """
            using System;
            using Guid = Other.Guid;

            namespace Other
            {
                struct Guid
                {
                    public static readonly Guid Empty = new Guid();
                }
            }

            class C
            {
                void M(Guid argument)
                {
                    if (argument.Equals(Guid.Empty)) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var diagnostics = await AnalyzerVerifier.GetDiagnosticsAsync(
            new ThrowIfEmptyGuidAnalyzer(),
            source,
            cancellationToken: cancellationToken
        );

        _ = await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task CodeFix_WhenApplied_ReplacesWithThrowIfEmptyGuidCall(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string source = """
            using System;

            class C
            {
                void M(Guid argument)
                {
                    if (argument == Guid.Empty) throw new ArgumentException(nameof(argument));
                }
            }
            """;

        var fixedSource = await AnalyzerVerifier.ApplyFixAsync(
            new ThrowIfEmptyGuidAnalyzer(),
            new ThrowIfEmptyGuidCodeFixProvider(),
            source,
            cancellationToken: cancellationToken
        );

        const string expected = """
            using System;

            class C
            {
                void M(Guid argument)
                {
                    ArgumentException.ThrowIfEmptyGuid(argument);
                }
            }
            """;

        _ = await Assert.That(fixedSource).IsEqualTo(expected);
    }
}
