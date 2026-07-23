namespace NetEvolve.Arguments.Analyser;

using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>Shared syntax- and semantic-model helpers used by every analyzer and code fix in this package.</summary>
internal static class SyntaxHelpers
{
    /// <summary>
    /// Determines whether the compilation's BCL already exposes the given static throw-helper member.
    /// Used to stay silent where the built-in CA1510/CA1511/CA1512 analyzers already apply, avoiding duplicate diagnostics.
    /// </summary>
    /// <param name="compilation">The compilation to look up the member in.</param>
    /// <param name="typeMetadataName">The fully-qualified metadata name of the declaring type, e.g. <c>System.ArgumentNullException</c>.</param>
    /// <param name="memberName">The name of the static member to look for, e.g. <c>ThrowIfNull</c>.</param>
    /// <returns><see langword="true"/> if the type exists in the compilation and declares a static member with that name; otherwise, <see langword="false"/>.</returns>
    public static bool HasBuiltInMember(Compilation compilation, string typeMetadataName, string memberName)
    {
        var type = compilation.GetTypeByMetadataName(typeMetadataName);

        return type is not null && type.GetMembers(memberName).Any(member => member.IsStatic);
    }

    /// <summary>Strips any enclosing parentheses from an expression.</summary>
    /// <param name="expression">The expression to unwrap.</param>
    /// <returns>The innermost expression once every enclosing <see cref="ParenthesizedExpressionSyntax"/> has been removed.</returns>
    public static ExpressionSyntax Unwrap(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
        {
            expression = parenthesized.Expression;
        }

        return expression;
    }

    /// <summary>Gets the single <c>throw</c> statement a statement consists of, whether written directly or as the sole statement of a block.</summary>
    /// <param name="statement">The statement to inspect, typically the body of an <c>if</c> statement.</param>
    /// <returns>The <see cref="ThrowStatementSyntax"/> if <paramref name="statement"/> is a throw statement, or a block containing exactly one; otherwise, <see langword="null"/>.</returns>
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

    /// <summary>Determines whether an object-creation expression constructs exactly the given exception type.</summary>
    /// <param name="semanticModel">The semantic model used to resolve the created type.</param>
    /// <param name="objectCreation">The <c>new</c> expression to inspect.</param>
    /// <param name="fullyQualifiedMetadataName">The fully-qualified metadata name of the expected exception type, e.g. <c>System.ArgumentException</c>.</param>
    /// <param name="cancellationToken">The token used to cancel semantic-model lookups.</param>
    /// <returns><see langword="true"/> if <paramref name="objectCreation"/> constructs exactly the named type; otherwise, <see langword="false"/>.</returns>
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

    /// <summary>
    /// Recognizes an <c>if</c> condition that is true precisely when an expression is <see langword="null"/> — covering
    /// <c>is null</c>/<c>is not null</c>, <c>==</c>/<c>!=</c>, <c>ReferenceEquals</c>, and any number of enclosing <c>!</c> negations.
    /// </summary>
    /// <param name="condition">The <c>if</c> statement's condition expression.</param>
    /// <param name="argument">When this method returns <see langword="true"/>, the expression being null-checked; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="condition"/> is a recognized "argument is null" shape; otherwise, <see langword="false"/>.</returns>
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

    /// <summary>
    /// Matches a single (non-negated) null-check shape and reports both the checked expression and whether the shape
    /// itself means "is null" (as opposed to "is not null") before any surrounding <c>!</c> negation is applied.
    /// </summary>
    /// <param name="condition">The already-unwrapped, not-yet-negation-adjusted condition expression.</param>
    /// <param name="argument">When this method returns <see langword="true"/>, the expression being checked; otherwise, <see langword="null"/>.</param>
    /// <param name="meansNull">When this method returns <see langword="true"/>, <see langword="true"/> if the shape means "is null", or <see langword="false"/> if it means "is not null".</param>
    /// <returns><see langword="true"/> if <paramref name="condition"/> matches a recognized null-check shape; otherwise, <see langword="false"/>.</returns>
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

    /// <summary>Recognizes a null-coalescing throw expression of the shape <c>arg ?? throw new ArgumentNullException(nameof(arg))</c>.</summary>
    /// <param name="semanticModel">The semantic model used to resolve the thrown exception's type.</param>
    /// <param name="binary">The coalesce (<c>??</c>) expression to inspect.</param>
    /// <param name="cancellationToken">The token used to cancel semantic-model lookups.</param>
    /// <param name="argument">When this method returns <see langword="true"/>, the left-hand side of the coalesce expression; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="binary"/> is a recognized coalescing null-check-and-throw shape; otherwise, <see langword="false"/>.</returns>
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

    /// <summary>Determines whether an expression is the <see langword="null"/> literal, ignoring enclosing parentheses.</summary>
    /// <param name="expression">The expression to test.</param>
    /// <returns><see langword="true"/> if <paramref name="expression"/> is the <see langword="null"/> literal; otherwise, <see langword="false"/>.</returns>
    public static bool IsNullLiteral(ExpressionSyntax expression) =>
        Unwrap(expression).IsKind(SyntaxKind.NullLiteralExpression);

    /// <summary>Determines whether an expression refers to a member or identifier named <c>ReferenceEquals</c>, regardless of qualification.</summary>
    /// <param name="expression">The invocation target expression to test.</param>
    /// <returns><see langword="true"/> if <paramref name="expression"/> is <c>ReferenceEquals</c> or <c>*.ReferenceEquals</c>; otherwise, <see langword="false"/>.</returns>
    private static bool IsReferenceEqualsName(ExpressionSyntax expression) =>
        Unwrap(expression) switch
        {
            IdentifierNameSyntax { Identifier.Text: "ReferenceEquals" } => true,
            MemberAccessExpressionSyntax { Name.Identifier.Text: "ReferenceEquals" } => true,
            _ => false,
        };

    /// <summary>Determines whether an expression is a numeric literal whose value is zero.</summary>
    /// <param name="expression">The expression to test.</param>
    /// <returns><see langword="true"/> if <paramref name="expression"/> is a numeric literal equal to zero; otherwise, <see langword="false"/>.</returns>
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

    /// <summary>Determines whether an expression is the <c>default</c> literal or an explicitly-typed <c>default(T)</c> expression.</summary>
    /// <param name="expression">The expression to test.</param>
    /// <returns><see langword="true"/> if <paramref name="expression"/> is <c>default</c> or <c>default(T)</c>; otherwise, <see langword="false"/>.</returns>
    public static bool IsDefaultLiteral(ExpressionSyntax expression) =>
        Unwrap(expression) switch
        {
            LiteralExpressionSyntax literal => literal.IsKind(SyntaxKind.DefaultLiteralExpression),
            DefaultExpressionSyntax => true,
            _ => false,
        };

    /// <summary>
    /// Determines whether an exception constructor's argument list is either empty, or a single argument that names
    /// <paramref name="argumentTarget"/> — via <c>nameof(argumentTarget)</c> or a matching string literal. Constructor
    /// calls with any other argument shape (e.g. a custom message) are rejected, since the throw-helper methods this
    /// analyzer package targets don't support one.
    /// </summary>
    /// <param name="argumentTarget">The expression being validated, whose name the constructor argument must match.</param>
    /// <param name="argumentList">The exception constructor's argument list.</param>
    /// <returns><see langword="true"/> if the argument list is empty or names <paramref name="argumentTarget"/>; otherwise, <see langword="false"/>.</returns>
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

    /// <summary>Determines whether two expressions are textually equivalent, used to check that a <c>nameof(...)</c> target matches the checked argument.</summary>
    /// <param name="left">The first expression.</param>
    /// <param name="right">The second expression.</param>
    /// <returns><see langword="true"/> if both expressions render to the same source text; otherwise, <see langword="false"/>.</returns>
    private static bool AreSameReference(ExpressionSyntax left, ExpressionSyntax right) => AreEquivalent(left, right);

    /// <summary>Determines whether two expressions are textually equivalent, used to check that both sides of a combined-range condition target the same value.</summary>
    /// <param name="left">The first expression.</param>
    /// <param name="right">The second expression.</param>
    /// <returns><see langword="true"/> if both expressions render to the same source text; otherwise, <see langword="false"/>.</returns>
    public static bool AreEquivalent(ExpressionSyntax left, ExpressionSyntax right) =>
        left.ToString() == right.ToString();
}
