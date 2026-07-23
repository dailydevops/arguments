namespace NetEvolve.Arguments.Analyser;

using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>Describes a recognized comparison-then-throw shape and the throw-helper member/arguments it maps to.</summary>
internal readonly struct ComparisonResult
{
    /// <summary>Initializes a new instance of the <see cref="ComparisonResult"/> struct.</summary>
    /// <param name="helperName">The name of the throw-helper member the comparison maps to.</param>
    /// <param name="valueExpression">The expression being validated (the left-hand side of the original comparison).</param>
    /// <param name="otherExpression">The bound the value is compared against, or <see langword="null"/> for zero-only helpers such as <c>ThrowIfZero</c>.</param>
    /// <param name="otherExpression2">The upper bound for combined-range comparisons (e.g. <c>ThrowIfOutOfRange</c>), or <see langword="null"/> otherwise.</param>
    public ComparisonResult(
        string helperName,
        ExpressionSyntax valueExpression,
        ExpressionSyntax? otherExpression,
        ExpressionSyntax? otherExpression2 = null
    )
    {
        HelperName = helperName;
        ValueExpression = valueExpression;
        OtherExpression = otherExpression;
        OtherExpression2 = otherExpression2;
    }

    /// <summary>Gets the name of the throw-helper member the comparison maps to, e.g. <c>ThrowIfNegative</c>.</summary>
    public string HelperName { get; }

    /// <summary>Gets the expression being validated (the left-hand side of the original comparison).</summary>
    public ExpressionSyntax ValueExpression { get; }

    /// <summary>Gets the bound the value is compared against, or <see langword="null"/> for zero-only helpers such as <c>ThrowIfZero</c>.</summary>
    public ExpressionSyntax? OtherExpression { get; }

    /// <summary>Gets the upper bound for combined-range comparisons (e.g. <c>ThrowIfOutOfRange</c>), or <see langword="null"/> otherwise.</summary>
    public ExpressionSyntax? OtherExpression2 { get; }
}
