namespace NetEvolve.Arguments.Analyser;

using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class SyntaxHelpers
{
    /// <summary>
    /// Determines whether the compilation's BCL already exposes the given static throw-helper member.
    /// Used to stay silent where the built-in CA1510/CA1511/CA1512 analyzers already apply, avoiding duplicate diagnostics.
    /// </summary>
    public static bool HasBuiltInMember(Compilation compilation, string typeMetadataName, string memberName)
    {
        var type = compilation.GetTypeByMetadataName(typeMetadataName);

        return type is not null && type.GetMembers(memberName).Any(member => member.IsStatic);
    }

    public static ExpressionSyntax Unwrap(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
        {
            expression = parenthesized.Expression;
        }

        return expression;
    }

    public static ThrowStatementSyntax? GetSingleThrowStatement(StatementSyntax statement)
    {
        if (statement is ThrowStatementSyntax throwStatement)
        {
            return throwStatement;
        }

        if (
            statement is BlockSyntax { Statements.Count: 1 } block
            && block.Statements[0] is ThrowStatementSyntax single
        )
        {
            return single;
        }

        return null;
    }

    public static bool IsExceptionType(
        SemanticModel semanticModel,
        ObjectCreationExpressionSyntax objectCreation,
        string fullyQualifiedMetadataName,
        CancellationToken cancellationToken
    )
    {
        var typeInfo = semanticModel.GetTypeInfo(objectCreation, cancellationToken);
        var exceptionType = semanticModel.Compilation.GetTypeByMetadataName(fullyQualifiedMetadataName);

        return exceptionType is not null && SymbolEqualityComparer.Default.Equals(typeInfo.Type, exceptionType);
    }

    public static bool TryGetNullCheckedExpression(ExpressionSyntax condition, out ExpressionSyntax? argument)
    {
        var negated = false;
        condition = Unwrap(condition);

        while (condition is PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.LogicalNotExpression } not)
        {
            negated = !negated;
            condition = Unwrap(not.Operand);
        }

        if (!TryGetNullCheckShape(condition, out argument, out var shapeMeansNull))
        {
            argument = null;
            return false;
        }

        if (shapeMeansNull == negated)
        {
            // "is null" negated, or "is not null" not negated -> the condition is true when non-null.
            argument = null;
            return false;
        }

        return true;
    }

    private static bool TryGetNullCheckShape(
        ExpressionSyntax condition,
        out ExpressionSyntax? argument,
        out bool meansNull
    )
    {
        switch (condition)
        {
            case IsPatternExpressionSyntax
            {
                Pattern: ConstantPatternSyntax { Expression: LiteralExpressionSyntax literal },
            } isPattern when literal.IsKind(SyntaxKind.NullLiteralExpression):
                argument = isPattern.Expression;
                meansNull = true;
                return true;

            case IsPatternExpressionSyntax
            {
                Pattern: UnaryPatternSyntax
                {
                    RawKind: (int)SyntaxKind.NotPattern,
                    Pattern: ConstantPatternSyntax { Expression: LiteralExpressionSyntax notLiteral },
                },
            } isNotPattern when notLiteral.IsKind(SyntaxKind.NullLiteralExpression):
                argument = isNotPattern.Expression;
                meansNull = false;
                return true;

            case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.EqualsExpression):
                if (IsNullLiteral(binary.Left))
                {
                    argument = binary.Right;
                    meansNull = true;
                    return true;
                }

                if (IsNullLiteral(binary.Right))
                {
                    argument = binary.Left;
                    meansNull = true;
                    return true;
                }

                break;

            case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.NotEqualsExpression):
                if (IsNullLiteral(binary.Left))
                {
                    argument = binary.Right;
                    meansNull = false;
                    return true;
                }

                if (IsNullLiteral(binary.Right))
                {
                    argument = binary.Left;
                    meansNull = false;
                    return true;
                }

                break;

            case InvocationExpressionSyntax { ArgumentList.Arguments.Count: 2 } referenceEqualsInvocation
                when IsReferenceEqualsName(referenceEqualsInvocation.Expression):
                var first = referenceEqualsInvocation.ArgumentList.Arguments[0].Expression;
                var second = referenceEqualsInvocation.ArgumentList.Arguments[1].Expression;

                if (IsNullLiteral(first))
                {
                    argument = second;
                    meansNull = true;
                    return true;
                }

                if (IsNullLiteral(second))
                {
                    argument = first;
                    meansNull = true;
                    return true;
                }

                break;
        }

        argument = null;
        meansNull = false;
        return false;
    }

    public static bool TryGetCoalesceNullCheck(
        SemanticModel semanticModel,
        BinaryExpressionSyntax binary,
        CancellationToken cancellationToken,
        out ExpressionSyntax? argument
    )
    {
        argument = null;

        if (!binary.IsKind(SyntaxKind.CoalesceExpression))
        {
            return false;
        }

        if (
            Unwrap(binary.Right)
            is not ThrowExpressionSyntax { Expression: ObjectCreationExpressionSyntax objectCreation }
        )
        {
            return false;
        }

        if (!IsExceptionType(semanticModel, objectCreation, "System.ArgumentNullException", cancellationToken))
        {
            return false;
        }

        if (objectCreation.ArgumentList is null || !IsSingleParamNameArgument(binary.Left, objectCreation.ArgumentList))
        {
            return false;
        }

        argument = binary.Left;
        return true;
    }

    public static bool IsNullLiteral(ExpressionSyntax expression) =>
        Unwrap(expression).IsKind(SyntaxKind.NullLiteralExpression);

    private static bool IsReferenceEqualsName(ExpressionSyntax expression) =>
        Unwrap(expression) switch
        {
            IdentifierNameSyntax { Identifier.Text: "ReferenceEquals" } => true,
            MemberAccessExpressionSyntax { Name.Identifier.Text: "ReferenceEquals" } => true,
            _ => false,
        };

    public static bool IsZeroLiteral(ExpressionSyntax expression)
    {
        if (
            Unwrap(expression) is not LiteralExpressionSyntax literal
            || !literal.IsKind(SyntaxKind.NumericLiteralExpression)
        )
        {
            return false;
        }

        var text = literal.Token.ValueText;
        return double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) && value == 0;
    }

    public static bool IsDefaultLiteral(ExpressionSyntax expression) =>
        Unwrap(expression) switch
        {
            LiteralExpressionSyntax literal => literal.IsKind(SyntaxKind.DefaultLiteralExpression),
            DefaultExpressionSyntax => true,
            _ => false,
        };

    public static bool IsSingleParamNameArgument(ExpressionSyntax argumentTarget, ArgumentListSyntax argumentList)
    {
        if (argumentList.Arguments.Count == 0)
        {
            return true;
        }

        if (argumentList.Arguments.Count != 1)
        {
            return false;
        }

        var argumentExpression = Unwrap(argumentList.Arguments[0].Expression);

        if (
            argumentExpression
                is InvocationExpressionSyntax
                {
                    Expression: IdentifierNameSyntax { Identifier.Text: "nameof" }
                } nameofInvocation
            && nameofInvocation.ArgumentList.Arguments.Count == 1
        )
        {
            var nameofTarget = Unwrap(nameofInvocation.ArgumentList.Arguments[0].Expression);
            return AreSameReference(nameofTarget, argumentTarget);
        }

        if (
            argumentExpression is LiteralExpressionSyntax { Token.Value: string literalText }
            && argumentTarget is IdentifierNameSyntax identifierName
        )
        {
            return string.Equals(literalText, identifierName.Identifier.Text, StringComparison.Ordinal);
        }

        return false;
    }

    private static bool AreSameReference(ExpressionSyntax left, ExpressionSyntax right) => AreEquivalent(left, right);

    public static bool AreEquivalent(ExpressionSyntax left, ExpressionSyntax right) =>
        left.ToString() == right.ToString();
}
