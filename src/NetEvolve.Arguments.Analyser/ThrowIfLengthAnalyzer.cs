namespace NetEvolve.Arguments.Analyser;

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>Reports string-length-comparison-then-throw patterns that can be replaced by an <c>ArgumentException</c> throw-helper.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ThrowIfLengthAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The fully-qualified metadata name of <see cref="ArgumentException"/>.</summary>
    private const string ArgumentExceptionMetadataName = "System.ArgumentException";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.ThrowIfLength);

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

    /// <summary>Analyzes an <c>if</c> statement and reports NEA0006 when it is a string-length-comparison-then-throw of <see cref="ArgumentException"/>.</summary>
    /// <param name="context">The syntax-node analysis context for the <c>if</c> statement being visited.</param>
    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;

        if (!TryGetLengthComparison(ifStatement.Condition, out var comparison) || comparison is null)
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
            Diagnostic.Create(DiagnosticDescriptors.ThrowIfLength, ifStatement.GetLocation(), value.HelperName, args)
        );
    }

    /// <summary>Recognizes <c>arg.Length &gt; max</c>, <c>arg.Length &lt; min</c>, and the combined range <c>arg.Length &lt; min || arg.Length &gt; max</c>.</summary>
    /// <param name="condition">The <c>if</c> statement's condition expression.</param>
    /// <param name="comparison">When this method returns <see langword="true"/>, the recognized comparison; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="condition"/> is a recognized string-length comparison shape; otherwise, <see langword="false"/>.</returns>
    internal static bool TryGetLengthComparison(ExpressionSyntax condition, out ComparisonResult? comparison)
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
                && TryGetLengthTarget(lessThan.Left, out var target1)
                && TryGetLengthTarget(greaterThan.Left, out var target2)
                && SyntaxHelpers.AreEquivalent(target1!, target2!)
            )
            {
                comparison = new ComparisonResult(
                    "ThrowIfLengthOutOfRange",
                    target1!,
                    lessThan.Right,
                    greaterThan.Right
                );
                return true;
            }

            return false;
        }

        if (condition is not BinaryExpressionSyntax binary || !TryGetLengthTarget(binary.Left, out var target))
        {
            return false;
        }

        comparison = binary.Kind() switch
        {
            SyntaxKind.GreaterThanExpression => new ComparisonResult("ThrowIfLengthGreaterThan", target!, binary.Right),
            SyntaxKind.LessThanExpression => new ComparisonResult("ThrowIfLengthLessThan", target!, binary.Right),
            _ => null,
        };

        return comparison is not null;
    }

    /// <summary>Recognizes a <c>.Length</c> property access and reports its qualifying expression.</summary>
    /// <param name="expression">The expression to test, typically one side of a comparison.</param>
    /// <param name="target">When this method returns <see langword="true"/>, the expression the <c>.Length</c> property was accessed on; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="expression"/> is a <c>.Length</c> access; otherwise, <see langword="false"/>.</returns>
    private static bool TryGetLengthTarget(ExpressionSyntax expression, out ExpressionSyntax? target)
    {
        if (SyntaxHelpers.Unwrap(expression) is MemberAccessExpressionSyntax { Name.Identifier.Text: "Length" } access)
        {
            target = access.Expression;
            return true;
        }

        target = null;
        return false;
    }
}
