namespace NetEvolve.Arguments.Analyser;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Shared helper for the <see cref="System.ArgumentException"/>-throwing analyzers, which all target
/// throw-helpers (<c>ThrowIfNullOrEmpty</c>, <c>ThrowIfNullOrWhiteSpace</c>, <c>ThrowIfContainsWhiteSpace</c>,
/// and the collection-count/string-length rules) that accept only a <c>paramName</c>, never a message.
/// </summary>
internal static class ArgumentExceptionParamNameHelpers
{
    /// <summary>
    /// Determines whether an <see cref="System.ArgumentException"/> constructor's argument list matches the shape
    /// required by this package's <c>ArgumentException</c>-based throw-helpers: either the shape already accepted
    /// by <see cref="SyntaxHelpers.IsSingleParamNameArgument"/> (empty, or a single argument naming
    /// <paramref name="argumentTarget"/>), or the common two-argument <c>ArgumentException(message, paramName)</c>
    /// constructor where <c>message</c> is the empty-string literal — a message that conveys no information — and
    /// <c>paramName</c> names <paramref name="argumentTarget"/>. Any other two-argument call (a real, non-empty
    /// message) is rejected, since the throw-helper methods this analyzer package targets don't support a message.
    /// </summary>
    /// <param name="argumentTarget">The expression being validated, whose name the constructor's <c>paramName</c> argument must match.</param>
    /// <param name="argumentList">The <see cref="System.ArgumentException"/> constructor's argument list.</param>
    /// <returns><see langword="true"/> if the argument list matches one of the accepted shapes; otherwise, <see langword="false"/>.</returns>
    public static bool IsSingleParamNameOrEmptyMessageArgument(
        ExpressionSyntax argumentTarget,
        ArgumentListSyntax argumentList
    )
    {
        if (SyntaxHelpers.IsSingleParamNameArgument(argumentTarget, argumentList))
        {
            return true;
        }

        if (argumentList.Arguments.Count != 2)
        {
            return false;
        }

        if (
            SyntaxHelpers.Unwrap(argumentList.Arguments[0].Expression)
                is not LiteralExpressionSyntax { Token.ValueText: "" } literal
            || !literal.IsKind(SyntaxKind.StringLiteralExpression)
        )
        {
            return false;
        }

        var paramNameOnlyList = SyntaxFactory.ArgumentList(
            SyntaxFactory.SingletonSeparatedList(argumentList.Arguments[1])
        );

        return SyntaxHelpers.IsSingleParamNameArgument(argumentTarget, paramNameOnlyList);
    }
}
