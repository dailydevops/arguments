namespace NetEvolve.Arguments.Analyser;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>Reports comparison-then-throw patterns that can be replaced by an <c>ArgumentOutOfRangeException</c> throw-helper.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ThrowIfOutOfRangeAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The fully-qualified metadata name of <see cref="ArgumentOutOfRangeException"/>.</summary>
    private const string ArgumentOutOfRangeExceptionMetadataName = "System.ArgumentOutOfRangeException";

    /// <summary>The fully-qualified metadata name of the open generic <see cref="IEquatable{T}"/> interface.</summary>
    private const string EquatableMetadataName = "System.IEquatable`1";

    /// <summary>The fully-qualified metadata name of the open generic <see cref="IComparable{T}"/> interface.</summary>
    private const string ComparableMetadataName = "System.IComparable`1";

    /// <summary>The fully-qualified metadata name of <c>System.Half</c>.</summary>
    private const string HalfMetadataName = "System.Half";

    /// <summary>
    /// The throw-helper members whose implementation relies on <see cref="IComparable{T}.CompareTo"/>. For IEEE
    /// floating-point operands (<see cref="float"/>, <see cref="double"/>, <c>System.Half</c>), <c>CompareTo</c>
    /// disagrees with the relational operators on <c>NaN</c>: e.g. <c>double.NaN.CompareTo(5.0)</c> is negative,
    /// while <c>double.NaN &lt; 5.0</c> is <see langword="false"/>. Rewriting such a comparison into the matching
    /// throw-helper would silently change which values throw, so these helpers are excluded for those operand types.
    /// <c>ThrowIfEqual</c>/<c>ThrowIfNotEqual</c> (backed by <see cref="EqualityComparer{T}"/>) and <c>ThrowIfZero</c>
    /// (backed by <c>INumberBase&lt;T&gt;.IsZero</c>) do not have this divergence and are intentionally not listed.
    /// </summary>
    private static readonly ImmutableHashSet<string> NaNSensitiveHelperNames = ImmutableHashSet.Create(
        StringComparer.Ordinal,
        "ThrowIfNegative",
        "ThrowIfNegativeOrZero",
        "ThrowIfLessThan",
        "ThrowIfLessThanOrEqual",
        "ThrowIfGreaterThan",
        "ThrowIfGreaterThanOrEqual",
        "ThrowIfOutOfRange"
    );

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.ThrowIfOutOfRange);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    /// <summary>Registers the syntax-node action for this rule, unless the compilation's BCL already exposes the <see cref="ArgumentOutOfRangeException"/> throw-helpers.</summary>
    /// <param name="context">The compilation-start context supplied by the Roslyn analyzer driver.</param>
    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        // The ArgumentOutOfRangeException throw-helpers exist on the BCL since .NET 8; where they do,
        // the built-in CA1512 analyzer already covers this pattern, so stay silent to avoid duplicates.
        if (SyntaxHelpers.HasBuiltInMember(context.Compilation, ArgumentOutOfRangeExceptionMetadataName, "ThrowIfZero"))
        {
            return;
        }

        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.IfStatement);
    }

    /// <summary>Analyzes an <see langword="if"/> statement and reports NEA0003 when it is a comparison-then-throw of <see cref="ArgumentOutOfRangeException"/>.</summary>
    /// <param name="context">The syntax-node analysis context for the <see langword="if"/> statement being visited.</param>
    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;

        if (!TryGetComparison(ifStatement.Condition, out var comparison) || comparison is null)
        {
            return;
        }

        if (
            NaNSensitiveHelperNames.Contains(comparison.Value.HelperName)
            && IsIeeeFloatingPointOperand(
                context.SemanticModel,
                comparison.Value.ValueExpression,
                context.CancellationToken
            )
        )
        {
            return;
        }

        if (
            !SyntaxHelpers.TryGetThrownException(
                ifStatement,
                context.SemanticModel,
                ArgumentOutOfRangeExceptionMetadataName,
                context.CancellationToken,
                out var objectCreation
            ) || objectCreation!.ArgumentList is null
        )
        {
            return;
        }

        if (!IsSupportedOperandType(comparison.Value, context.SemanticModel, context.CancellationToken))
        {
            return;
        }

        var value = comparison.Value;

        if (!SyntaxHelpers.IsSingleParamNameArgument(value.ValueExpression, objectCreation.ArgumentList))
        {
            return;
        }

        string args;

        if (value.OtherExpression2 is not null)
        {
            args = $"{value.ValueExpression}, {value.OtherExpression}, {value.OtherExpression2}";
        }
        else if (value.OtherExpression is not null)
        {
            args = $"{value.ValueExpression}, {value.OtherExpression}";
        }
        else
        {
            args = value.ValueExpression.ToString();
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticDescriptors.ThrowIfOutOfRange,
                ifStatement.GetLocation(),
                value.HelperName,
                args
            )
        );
    }

    /// <summary>
    /// Determines whether the compared operand's type actually satisfies the generic constraint of the
    /// <see cref="ArgumentOutOfRangeException"/> throw-helper the comparison would be rewritten to, so that
    /// the rewritten call is guaranteed to compile.
    /// </summary>
    /// <param name="comparison">The recognized comparison.</param>
    /// <param name="semanticModel">The semantic model used to resolve the operand's type.</param>
    /// <param name="cancellationToken">The token used to cancel type resolution.</param>
    /// <returns><see langword="true"/> if the operand type implements the required generic interface closed over itself; otherwise, <see langword="false"/>.</returns>
    private static bool IsSupportedOperandType(
        ComparisonResult comparison,
        SemanticModel semanticModel,
        CancellationToken cancellationToken
    )
    {
        var operandType = semanticModel.GetTypeInfo(comparison.ValueExpression, cancellationToken).Type;

        if (operandType is null || operandType.TypeKind == TypeKind.Error)
        {
            return false;
        }

        var requiredInterfaceMetadataName = comparison.HelperName is "ThrowIfEqual" or "ThrowIfNotEqual"
            ? EquatableMetadataName
            : ComparableMetadataName;

        var unconstructedRequiredInterface = semanticModel.Compilation.GetTypeByMetadataName(
            requiredInterfaceMetadataName
        );

        if (unconstructedRequiredInterface is null)
        {
            return false;
        }

        var requiredInterface = unconstructedRequiredInterface.Construct(operandType);

        return operandType.AllInterfaces.Any(implementedInterface =>
            SymbolEqualityComparer.Default.Equals(implementedInterface, requiredInterface)
        );
    }

    /// <summary>
    /// Determines whether an operand's type is an IEEE floating-point type (<see cref="float"/>, <see cref="double"/>,
    /// or <c>System.Half</c>) — the types for which <see cref="IComparable{T}.CompareTo"/> and the relational
    /// operators disagree on <c>NaN</c>.
    /// </summary>
    /// <param name="semanticModel">The semantic model used to resolve the operand's type.</param>
    /// <param name="operand">The compared expression whose type is inspected.</param>
    /// <param name="cancellationToken">The token used to cancel semantic-model lookups.</param>
    /// <returns><see langword="true"/> if <paramref name="operand"/>'s type is <see cref="float"/>, <see cref="double"/>, or <c>System.Half</c>; otherwise, <see langword="false"/>.</returns>
    private static bool IsIeeeFloatingPointOperand(
        SemanticModel semanticModel,
        ExpressionSyntax operand,
        CancellationToken cancellationToken
    )
    {
        var type = semanticModel.GetTypeInfo(operand, cancellationToken).Type;

        if (type is null)
        {
            return false;
        }

        if (type.SpecialType is SpecialType.System_Single or SpecialType.System_Double)
        {
            return true;
        }

        // System.Half has no SpecialType and may not exist on older target frameworks, so a null lookup is normal.
        var halfType = semanticModel.Compilation.GetTypeByMetadataName(HalfMetadataName);

        return halfType is not null && SymbolEqualityComparer.Default.Equals(type, halfType);
    }

    /// <summary>
    /// Recognizes a comparison against zero, another expression, or a combined range (<c>value &lt; min || value &gt; max</c>)
    /// and maps it to the matching <see cref="ArgumentOutOfRangeException"/> throw-helper member and arguments.
    /// </summary>
    /// <param name="condition">The <see langword="if"/> statement's condition expression.</param>
    /// <param name="comparison">When this method returns <see langword="true"/>, the recognized comparison; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="condition"/> is a recognized comparison shape; otherwise, <see langword="false"/>.</returns>
    internal static bool TryGetComparison(ExpressionSyntax condition, out ComparisonResult? comparison)
    {
        condition = SyntaxHelpers.Unwrap(condition);
        comparison = null;

        if (condition is BinaryExpressionSyntax { RawKind: (int)SyntaxKind.LogicalOrExpression } orExpression)
        {
            return TryGetCombinedRangeComparison(orExpression, out comparison);
        }

        if (condition is not BinaryExpressionSyntax binary)
        {
            return false;
        }

        var value = binary.Left;
        var other = binary.Right;

        if (SyntaxHelpers.Unwrap(value) is LiteralExpressionSyntax)
        {
            return false;
        }

        var isZero = SyntaxHelpers.IsZeroLiteral(other);

        comparison = binary.Kind() switch
        {
            SyntaxKind.LessThanExpression when isZero => new ComparisonResult("ThrowIfNegative", value, null),
            SyntaxKind.LessThanExpression => new ComparisonResult("ThrowIfLessThan", value, other),
            SyntaxKind.LessThanOrEqualExpression when isZero => new ComparisonResult(
                "ThrowIfNegativeOrZero",
                value,
                null
            ),
            SyntaxKind.LessThanOrEqualExpression => new ComparisonResult("ThrowIfLessThanOrEqual", value, other),
            SyntaxKind.GreaterThanExpression => new ComparisonResult("ThrowIfGreaterThan", value, other),
            SyntaxKind.GreaterThanOrEqualExpression => new ComparisonResult("ThrowIfGreaterThanOrEqual", value, other),
            SyntaxKind.EqualsExpression when isZero => new ComparisonResult("ThrowIfZero", value, null),
            SyntaxKind.EqualsExpression => new ComparisonResult("ThrowIfEqual", value, other),
            SyntaxKind.NotEqualsExpression => new ComparisonResult("ThrowIfNotEqual", value, other),
            _ => null,
        };

        return comparison is not null;
    }

    /// <summary>Recognizes the combined-range shape <c>value &lt; min || value &gt; max</c> and maps it to <c>ThrowIfOutOfRange</c>.</summary>
    /// <param name="orExpression">The <c>||</c> expression to inspect.</param>
    /// <param name="comparison">When this method returns <see langword="true"/>, the recognized comparison; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="orExpression"/> is a recognized combined-range shape; otherwise, <see langword="false"/>.</returns>
    private static bool TryGetCombinedRangeComparison(
        BinaryExpressionSyntax orExpression,
        out ComparisonResult? comparison
    )
    {
        comparison = null;

        if (
            SyntaxHelpers.Unwrap(orExpression.Left)
                is not BinaryExpressionSyntax { RawKind: (int)SyntaxKind.LessThanExpression } lessThan
            || SyntaxHelpers.Unwrap(orExpression.Right)
                is not BinaryExpressionSyntax { RawKind: (int)SyntaxKind.GreaterThanExpression } greaterThan
        )
        {
            return false;
        }

        var lessThanValue = SyntaxHelpers.Unwrap(lessThan.Left);
        var greaterThanValue = SyntaxHelpers.Unwrap(greaterThan.Left);

        if (!IsSideEffectFreeOperand(lessThanValue) || !SyntaxHelpers.AreEquivalent(lessThanValue, greaterThanValue))
        {
            return false;
        }

        comparison = new ComparisonResult("ThrowIfOutOfRange", lessThanValue, lessThan.Right, greaterThan.Right);
        return true;
    }

    /// <summary>
    /// Determines whether an expression is safe to fold from two source occurrences (once on each side of the
    /// combined-range condition) into a single occurrence in the generated throw-helper call. The combined-range
    /// shape is the only one where the operand is textually duplicated in the source but emitted only once in the
    /// fix, so unlike the simple comparison shapes, evaluating it twice versus once can silently change behavior if
    /// the expression has a side effect (e.g. it advances an iterator or increments a counter).
    /// </summary>
    /// <remarks>
    /// This is a conservative, purely syntactic allow-list: a bare identifier (a parameter or local), or a chain of
    /// member accesses rooted in one. Invocations, indexers, object creation, <see langword="await"/>, and assignment or
    /// increment/decrement expressions are all rejected. Note that a member access is not strictly guaranteed to be
    /// side-effect-free either, since a property getter can run arbitrary code; however, rejecting all member access
    /// would gut the rule's main use case (e.g. <c>arg.Length</c>), so accepting member-access chains while
    /// rejecting invocations is the pragmatic line drawn here.
    /// </remarks>
    /// <param name="expression">The already-unwrapped operand expression to test.</param>
    /// <returns><see langword="true"/> if <paramref name="expression"/> is an identifier or a member-access chain rooted in one; otherwise, <see langword="false"/>.</returns>
    private static bool IsSideEffectFreeOperand(ExpressionSyntax expression)
    {
        while (expression is MemberAccessExpressionSyntax memberAccess)
        {
            expression = SyntaxHelpers.Unwrap(memberAccess.Expression);
        }

        return expression is IdentifierNameSyntax;
    }
}
