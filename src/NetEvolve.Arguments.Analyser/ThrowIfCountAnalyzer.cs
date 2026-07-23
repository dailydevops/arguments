namespace NetEvolve.Arguments.Analyser;

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>Reports collection-count-comparison-then-throw patterns that can be replaced by an <c>ArgumentException</c> throw-helper.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ThrowIfCountAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The fully-qualified metadata name of <see cref="ArgumentException"/>.</summary>
    private const string ArgumentExceptionMetadataName = "System.ArgumentException";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.ThrowIfCount);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.IfStatement);
    }

    /// <summary>Analyzes an <c>if</c> statement and reports NEA0007 when it is a collection-count-comparison-then-throw of <see cref="ArgumentException"/>.</summary>
    /// <param name="context">The syntax-node analysis context for the <c>if</c> statement being visited.</param>
    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;

        if (!TryGetCountComparison(ifStatement.Condition, out var comparison) || comparison is null)
        {
            return;
        }

        if (
            !SyntaxHelpers.TryGetThrownException(
                ifStatement,
                context.SemanticModel,
                ArgumentExceptionMetadataName,
                context.CancellationToken,
                out _
            )
        )
        {
            return;
        }

        var value = comparison.Value;
        var args = value.OtherExpression2 is null
            ? $"{value.ValueExpression}, {value.OtherExpression}"
            : $"{value.ValueExpression}, {value.OtherExpression}, {value.OtherExpression2}";

        context.ReportDiagnostic(
            Diagnostic.Create(DiagnosticDescriptors.ThrowIfCount, ifStatement.GetLocation(), value.HelperName, args)
        );
    }

    /// <summary>Recognizes <c>arg.Count &gt; max</c>, <c>arg.Count &lt; min</c>, and the combined range <c>arg.Count &lt; min || arg.Count &gt; max</c> (both the <c>.Count</c> property and the <c>.Count()</c> LINQ extension method).</summary>
    /// <param name="condition">The <c>if</c> statement's condition expression.</param>
    /// <param name="comparison">When this method returns <see langword="true"/>, the recognized comparison; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="condition"/> is a recognized collection-count comparison shape; otherwise, <see langword="false"/>.</returns>
    internal static bool TryGetCountComparison(ExpressionSyntax condition, out ComparisonResult? comparison)
    {
        condition = SyntaxHelpers.Unwrap(condition);
        comparison = null;

        if (condition is BinaryExpressionSyntax { RawKind: (int)SyntaxKind.LogicalOrExpression } orExpression)
        {
            if (
                SyntaxHelpers.Unwrap(orExpression.Left)
                    is BinaryExpressionSyntax { RawKind: (int)SyntaxKind.LessThanExpression } lessThan
                && SyntaxHelpers.Unwrap(orExpression.Right)
                    is BinaryExpressionSyntax { RawKind: (int)SyntaxKind.GreaterThanExpression } greaterThan
                && TryGetCountTarget(lessThan.Left, out var target1)
                && TryGetCountTarget(greaterThan.Left, out var target2)
                && SyntaxHelpers.AreEquivalent(target1!, target2!)
            )
            {
                comparison = new ComparisonResult(
                    "ThrowIfCountOutOfRange",
                    target1!,
                    lessThan.Right,
                    greaterThan.Right
                );
                return true;
            }

            return false;
        }

        if (condition is not BinaryExpressionSyntax binary || !TryGetCountTarget(binary.Left, out var target))
        {
            return false;
        }

        comparison = binary.Kind() switch
        {
            SyntaxKind.GreaterThanExpression => new ComparisonResult("ThrowIfCountGreaterThan", target!, binary.Right),
            SyntaxKind.LessThanExpression => new ComparisonResult("ThrowIfCountLessThan", target!, binary.Right),
            _ => null,
        };

        return comparison is not null;
    }

    /// <summary>Recognizes a <c>.Count</c> property access or a parameterless <c>.Count()</c> LINQ extension method call, and reports its qualifying expression.</summary>
    /// <param name="expression">The expression to test, typically one side of a comparison.</param>
    /// <param name="target">When this method returns <see langword="true"/>, the expression the count was taken of; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="expression"/> is a recognized count-access shape; otherwise, <see langword="false"/>.</returns>
    private static bool TryGetCountTarget(ExpressionSyntax expression, out ExpressionSyntax? target)
    {
        var unwrapped = SyntaxHelpers.Unwrap(expression);

        if (unwrapped is MemberAccessExpressionSyntax { Name.Identifier.Text: "Count" } access)
        {
            target = access.Expression;
            return true;
        }

        if (
            unwrapped is InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "Count" } countAccess,
                ArgumentList.Arguments.Count: 0,
            }
        )
        {
            target = countAccess.Expression;
            return true;
        }

        target = null;
        return false;
    }
}
