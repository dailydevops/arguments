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

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;

        if (ifStatement.Else is not null)
        {
            return;
        }

        if (!TryGetCountComparison(ifStatement.Condition, out var comparison) || comparison is null)
        {
            return;
        }

        var throwStatement = SyntaxHelpers.GetSingleThrowStatement(ifStatement.Statement);

        if (throwStatement?.Expression is not ObjectCreationExpressionSyntax objectCreation)
        {
            return;
        }

        if (
            !SyntaxHelpers.IsExceptionType(
                context.SemanticModel,
                objectCreation,
                ArgumentExceptionMetadataName,
                context.CancellationToken
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
