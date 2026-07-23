namespace NetEvolve.Arguments.Analyser;

using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>Describes a recognized comparison-then-throw shape and the throw-helper member/arguments it maps to.</summary>
internal readonly struct ComparisonResult
{
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

    public string HelperName { get; }

    public ExpressionSyntax ValueExpression { get; }

    public ExpressionSyntax? OtherExpression { get; }

    public ExpressionSyntax? OtherExpression2 { get; }
}
