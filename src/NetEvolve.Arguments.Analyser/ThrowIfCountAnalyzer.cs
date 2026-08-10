namespace NetEvolve.Arguments.Analyser;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
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

    /// <summary>The fully-qualified metadata name of the open generic <c>IEnumerable&lt;T&gt;</c> interface.</summary>
    private const string EnumerableInterfaceMetadataName = "System.Collections.Generic.IEnumerable`1";

    /// <summary>The fully-qualified metadata name of <see cref="System.Linq.Enumerable"/>, the declaring type of the LINQ <c>Count()</c> extension method.</summary>
    private const string EnumerableTypeMetadataName = "System.Linq.Enumerable";

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

    /// <summary>Analyzes an <see langword="if"/> statement and reports NEA0007 when it is a collection-count-comparison-then-throw of <see cref="ArgumentException"/>.</summary>
    /// <param name="context">The syntax-node analysis context for the <see langword="if"/> statement being visited.</param>
    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;

        if (!TryGetCountComparison(ifStatement.Condition, out var comparison) || comparison is null)
        {
            return;
        }

        if (!IsSupportedCountAccess(comparison.Value.ValueExpression, context.SemanticModel, context.CancellationToken))
        {
            return;
        }

        if (
            !SyntaxHelpers.TryGetThrownException(
                ifStatement,
                context.SemanticModel,
                ArgumentExceptionMetadataName,
                context.CancellationToken,
                out var objectCreation
            ) || objectCreation!.ArgumentList is null
        )
        {
            return;
        }

        var value = comparison.Value;

        if (
            !ArgumentExceptionParamNameHelpers.IsSingleParamNameOrEmptyMessageArgument(
                value.ValueExpression,
                objectCreation.ArgumentList
            )
        )
        {
            return;
        }

        var args = value.OtherExpression2 is null
            ? $"{value.ValueExpression}, {value.OtherExpression}"
            : $"{value.ValueExpression}, {value.OtherExpression}, {value.OtherExpression2}";

        context.ReportDiagnostic(
            Diagnostic.Create(DiagnosticDescriptors.ThrowIfCount, ifStatement.GetLocation(), value.HelperName, args)
        );
    }

    /// <summary>Recognizes <c>arg.Count &gt; max</c>, <c>arg.Count &lt; min</c>, and the combined range <c>arg.Count &lt; min || arg.Count &gt; max</c> (both the <c>.Count</c> property and the <c>.Count()</c> LINQ extension method).</summary>
    /// <param name="condition">The <see langword="if"/> statement's condition expression.</param>
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

    /// <summary>
    /// Determines whether the resolved <c>.Count</c>/<c>.Count()</c> access rooted at <paramref name="target"/> is one
    /// the <c>ArgumentException.ThrowIfCount*</c> throw-helpers can actually bind to: the receiver must be an array or
    /// implement <c>IEnumerable&lt;T&gt;</c>, and a <c>.Count()</c> invocation must resolve to the LINQ
    /// <see cref="System.Linq.Enumerable"/>.<c>Count</c> extension method rather than an unrelated member also named
    /// <c>Count</c>.
    /// </summary>
    /// <param name="target">The receiver expression the count was taken of, as reported by <see cref="TryGetCountTarget"/>.</param>
    /// <param name="semanticModel">The semantic model used to resolve the receiver's type and the invoked <c>Count</c> symbol.</param>
    /// <param name="cancellationToken">The token used to cancel semantic-model lookups.</param>
    /// <returns><see langword="true"/> if the throw-helper substitution would compile; otherwise, <see langword="false"/>.</returns>
    private static bool IsSupportedCountAccess(
        ExpressionSyntax target,
        SemanticModel semanticModel,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsSupportedReceiverType(target, semanticModel, cancellationToken))
        {
            return false;
        }

        if (target.Parent is not MemberAccessExpressionSyntax { Name.Identifier.Text: "Count" } countAccess)
        {
            return false;
        }

        if (countAccess.Parent is not InvocationExpressionSyntax { ArgumentList.Arguments.Count: 0 } invocation)
        {
            // A plain ".Count" property access; no further symbol resolution is needed.
            return true;
        }

        var symbol = semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol;
        var enumerableType = semanticModel.Compilation.GetTypeByMetadataName(EnumerableTypeMetadataName);

        return symbol is IMethodSymbol methodSymbol
            && enumerableType is not null
            && SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, enumerableType);
    }

    /// <summary>
    /// Determines whether the given expression's type is one the <c>ArgumentException.ThrowIfCount*</c> throw-helpers
    /// have an overload for: an array, or a type that implements (or is) <c>IEnumerable&lt;T&gt;</c> for some <c>T</c>.
    /// </summary>
    /// <param name="target">The receiver expression whose type is checked.</param>
    /// <param name="semanticModel">The semantic model used to resolve the receiver's type.</param>
    /// <param name="cancellationToken">The token used to cancel semantic-model lookups.</param>
    /// <returns><see langword="true"/> if the receiver's type qualifies; otherwise, <see langword="false"/>.</returns>
    private static bool IsSupportedReceiverType(
        ExpressionSyntax target,
        SemanticModel semanticModel,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var type = semanticModel.GetTypeInfo(target, cancellationToken).Type;

        if (type is null or IErrorTypeSymbol)
        {
            return false;
        }

        if (type is IArrayTypeSymbol)
        {
            return true;
        }

        var enumerableInterface = semanticModel.Compilation.GetTypeByMetadataName(EnumerableInterfaceMetadataName);

        if (enumerableInterface is null)
        {
            return false;
        }

        if (
            type is INamedTypeSymbol namedType
            && SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, enumerableInterface)
        )
        {
            return true;
        }

        return type.AllInterfaces.Any(candidate =>
            SymbolEqualityComparer.Default.Equals(candidate.OriginalDefinition, enumerableInterface)
        );
    }
}
