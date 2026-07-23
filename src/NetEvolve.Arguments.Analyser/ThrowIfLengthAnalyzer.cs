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

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;

        if (ifStatement.Else is not null)
        {
            return;
        }

        if (!TryGetLengthComparison(ifStatement.Condition, out var comparison) || comparison is null)
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
            Diagnostic.Create(DiagnosticDescriptors.ThrowIfLength, ifStatement.GetLocation(), value.HelperName, args)
        );
    }

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
