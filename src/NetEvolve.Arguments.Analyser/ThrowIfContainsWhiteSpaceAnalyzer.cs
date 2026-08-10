namespace NetEvolve.Arguments.Analyser;

using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>Reports white-space-check-then-throw patterns that can be replaced by <c>ArgumentException.ThrowIfContainsWhiteSpace</c>.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ThrowIfContainsWhiteSpaceAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The fully-qualified metadata name of <see cref="ArgumentException"/>.</summary>
    private const string ArgumentExceptionMetadataName = "System.ArgumentException";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.ThrowIfContainsWhiteSpace);

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

    /// <summary>Analyzes an <see langword="if"/> statement and reports NEA0008 when it is a white-space-check-then-throw of <see cref="ArgumentException"/>.</summary>
    /// <param name="context">The syntax-node analysis context for the <see langword="if"/> statement being visited.</param>
    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;

        if (
            !TryGetContainsWhiteSpaceTarget(ifStatement.Condition, out var argument, out var invocationsToVerify)
            || argument is null
        )
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

        if (
            !ArgumentExceptionParamNameHelpers.IsSingleParamNameOrEmptyMessageArgument(
                argument,
                objectCreation.ArgumentList
            )
        )
        {
            return;
        }

        if (!IsStringReceiver(argument, context.SemanticModel, context.CancellationToken))
        {
            return;
        }

        foreach (var (invocation, expectedMethodName) in invocationsToVerify)
        {
            if (
                !IsLinqEnumerableMethod(
                    invocation,
                    expectedMethodName,
                    context.SemanticModel,
                    context.CancellationToken
                )
            )
            {
                return;
            }
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticDescriptors.ThrowIfContainsWhiteSpace,
                ifStatement.GetLocation(),
                argument.ToString()
            )
        );
    }

    /// <summary>
    /// Recognizes the white-space-check shapes supported by this rule:
    /// <list type="bullet">
    /// <item><description><c>arg.Any(c => char.IsWhiteSpace(c))</c> and the method-group form <c>arg.Any(char.IsWhiteSpace)</c>.</description></item>
    /// <item><description><c>arg.Count(char.IsWhiteSpace) &gt; 0</c>, <c>&gt;= 1</c> and <c>!= 0</c> (and the operand-swapped forms, e.g. <c>0 &lt; arg.Count(char.IsWhiteSpace)</c>).</description></item>
    /// <item><description><c>arg.Where(char.IsWhiteSpace).Any()</c>.</description></item>
    /// </list>
    /// </summary>
    /// <param name="condition">The <see langword="if"/> statement's condition expression.</param>
    /// <param name="argument">When this method returns <see langword="true"/>, the string argument being checked; otherwise, <see langword="null"/>.</param>
    /// <param name="invocationsToVerify">
    /// When this method returns <see langword="true"/>, the invocation(s) that must still be confirmed (via the
    /// semantic model) to resolve to the corresponding <see cref="System.Linq.Enumerable"/> method, paired with the
    /// expected method name; otherwise, empty.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="condition"/> is a recognized white-space-check shape; otherwise, <see langword="false"/>.</returns>
    internal static bool TryGetContainsWhiteSpaceTarget(
        ExpressionSyntax condition,
        out ExpressionSyntax? argument,
        out ImmutableArray<(InvocationExpressionSyntax Invocation, string ExpectedMethodName)> invocationsToVerify
    )
    {
        argument = null;
        invocationsToVerify = ImmutableArray<(InvocationExpressionSyntax, string)>.Empty;
        condition = SyntaxHelpers.Unwrap(condition);

        if (TryGetAnyShape(condition, out argument, out var anyInvocation))
        {
            invocationsToVerify = ImmutableArray.Create((anyInvocation!, "Any"));
            return true;
        }

        if (TryGetCountShape(condition, out argument, out var countInvocation))
        {
            invocationsToVerify = ImmutableArray.Create((countInvocation!, "Count"));
            return true;
        }

        if (TryGetWhereAnyShape(condition, out argument, out var whereInvocation, out var whereAnyInvocation))
        {
            invocationsToVerify = ImmutableArray.Create((whereInvocation!, "Where"), (whereAnyInvocation!, "Any"));
            return true;
        }

        return false;
    }

    /// <summary>Recognizes <c>arg.Any(c => char.IsWhiteSpace(c))</c> and the method-group form <c>arg.Any(char.IsWhiteSpace)</c>.</summary>
    /// <param name="condition">The already-unwrapped condition expression.</param>
    /// <param name="argument">When this method returns <see langword="true"/>, the string argument being checked; otherwise, <see langword="null"/>.</param>
    /// <param name="invocation">When this method returns <see langword="true"/>, the matched <c>Any</c> invocation; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="condition"/> is the recognized <c>Any</c> shape; otherwise, <see langword="false"/>.</returns>
    private static bool TryGetAnyShape(
        ExpressionSyntax condition,
        out ExpressionSyntax? argument,
        out InvocationExpressionSyntax? invocation
    )
    {
        argument = null;
        invocation = null;

        if (
            condition
            is not InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "Any" } access,
                ArgumentList.Arguments.Count: 1,
            } anyInvocation
        )
        {
            return false;
        }

        if (!IsWhiteSpacePredicate(anyInvocation.ArgumentList.Arguments[0].Expression))
        {
            return false;
        }

        argument = access.Expression;
        invocation = anyInvocation;
        return true;
    }

    /// <summary>
    /// Recognizes <c>arg.Count(char.IsWhiteSpace) &gt; 0</c>, <c>&gt;= 1</c> and <c>!= 0</c>, and the operand-swapped
    /// forms (e.g. <c>0 &lt; arg.Count(char.IsWhiteSpace)</c>), all of which mean "at least one character matches".
    /// </summary>
    /// <param name="condition">The already-unwrapped condition expression.</param>
    /// <param name="argument">When this method returns <see langword="true"/>, the string argument being checked; otherwise, <see langword="null"/>.</param>
    /// <param name="invocation">When this method returns <see langword="true"/>, the matched <c>Count</c> invocation; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="condition"/> is the recognized <c>Count</c> comparison shape; otherwise, <see langword="false"/>.</returns>
    private static bool TryGetCountShape(
        ExpressionSyntax condition,
        out ExpressionSyntax? argument,
        out InvocationExpressionSyntax? invocation
    )
    {
        argument = null;
        invocation = null;

        if (condition is not BinaryExpressionSyntax binary)
        {
            return false;
        }

        var left = SyntaxHelpers.Unwrap(binary.Left);
        var right = SyntaxHelpers.Unwrap(binary.Right);
        var kind = binary.Kind();

        if (TryGetCountInvocation(left, out var countInvocation) && MeansCountGreaterThanZero(kind, right))
        {
            invocation = countInvocation;
        }
        else if (TryGetCountInvocation(right, out countInvocation) && MeansZeroLessThanCount(kind, left))
        {
            invocation = countInvocation;
        }
        else
        {
            return false;
        }

        if (!IsWhiteSpacePredicate(invocation!.ArgumentList.Arguments[0].Expression))
        {
            invocation = null;
            return false;
        }

        argument = ((MemberAccessExpressionSyntax)invocation.Expression).Expression;
        return true;
    }

    /// <summary>Recognizes a single-argument <c>.Count(predicate)</c> LINQ extension method invocation.</summary>
    /// <param name="expression">The already-unwrapped expression to test.</param>
    /// <param name="invocation">When this method returns <see langword="true"/>, the matched invocation; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="expression"/> is a recognized <c>Count(predicate)</c> shape; otherwise, <see langword="false"/>.</returns>
    private static bool TryGetCountInvocation(ExpressionSyntax expression, out InvocationExpressionSyntax? invocation)
    {
        if (
            expression is InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "Count" },
                ArgumentList.Arguments.Count: 1,
            } countInvocation
        )
        {
            invocation = countInvocation;
            return true;
        }

        invocation = null;
        return false;
    }

    /// <summary>Determines whether <c>Count(...) kind rhs</c> means "at least one", i.e. <c>&gt; 0</c>, <c>&gt;= 1</c> or <c>!= 0</c>.</summary>
    private static bool MeansCountGreaterThanZero(SyntaxKind kind, ExpressionSyntax rhs) =>
        (kind == SyntaxKind.GreaterThanExpression && IsIntegerLiteral(rhs, 0))
        || (kind == SyntaxKind.GreaterThanOrEqualExpression && IsIntegerLiteral(rhs, 1))
        || (kind == SyntaxKind.NotEqualsExpression && IsIntegerLiteral(rhs, 0));

    /// <summary>Determines whether <c>lhs kind Count(...)</c> means "at least one", i.e. <c>0 &lt; ...</c>, <c>1 &lt;= ...</c> or <c>0 != ...</c>.</summary>
    private static bool MeansZeroLessThanCount(SyntaxKind kind, ExpressionSyntax lhs) =>
        (kind == SyntaxKind.LessThanExpression && IsIntegerLiteral(lhs, 0))
        || (kind == SyntaxKind.LessThanOrEqualExpression && IsIntegerLiteral(lhs, 1))
        || (kind == SyntaxKind.NotEqualsExpression && IsIntegerLiteral(lhs, 0));

    /// <summary>Determines whether an expression is an integer literal equal to <paramref name="value"/>.</summary>
    /// <param name="expression">The already-unwrapped expression to test.</param>
    /// <param name="value">The expected integer value.</param>
    /// <returns><see langword="true"/> if <paramref name="expression"/> is an integer literal equal to <paramref name="value"/>; otherwise, <see langword="false"/>.</returns>
    private static bool IsIntegerLiteral(ExpressionSyntax expression, int value) =>
        SyntaxHelpers.Unwrap(expression) is LiteralExpressionSyntax { Token.Value: int literalValue }
        && literalValue == value;

    /// <summary>Recognizes <c>arg.Where(char.IsWhiteSpace).Any()</c>, with a parameterless <c>Any()</c> call.</summary>
    /// <param name="condition">The already-unwrapped condition expression.</param>
    /// <param name="argument">When this method returns <see langword="true"/>, the string argument being checked; otherwise, <see langword="null"/>.</param>
    /// <param name="whereInvocation">When this method returns <see langword="true"/>, the matched <c>Where</c> invocation; otherwise, <see langword="null"/>.</param>
    /// <param name="anyInvocation">When this method returns <see langword="true"/>, the matched <c>Any</c> invocation; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="condition"/> is the recognized <c>Where(...).Any()</c> shape; otherwise, <see langword="false"/>.</returns>
    private static bool TryGetWhereAnyShape(
        ExpressionSyntax condition,
        out ExpressionSyntax? argument,
        out InvocationExpressionSyntax? whereInvocation,
        out InvocationExpressionSyntax? anyInvocation
    )
    {
        argument = null;
        whereInvocation = null;
        anyInvocation = null;

        if (
            condition
            is not InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "Any" } anyAccess,
                ArgumentList.Arguments.Count: 0,
            } anyInvocationCandidate
        )
        {
            return false;
        }

        if (
            SyntaxHelpers.Unwrap(anyAccess.Expression)
            is not InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "Where" } whereAccess,
                ArgumentList.Arguments.Count: 1,
            } whereInvocationCandidate
        )
        {
            return false;
        }

        if (!IsWhiteSpacePredicate(whereInvocationCandidate.ArgumentList.Arguments[0].Expression))
        {
            return false;
        }

        argument = whereAccess.Expression;
        whereInvocation = whereInvocationCandidate;
        anyInvocation = anyInvocationCandidate;
        return true;
    }

    /// <summary>Determines whether a predicate argument is <c>char.IsWhiteSpace</c> as a method group, or the equivalent lambda <c>c =&gt; char.IsWhiteSpace(c)</c>.</summary>
    /// <param name="predicateArgument">The argument expression passed as the predicate.</param>
    /// <returns><see langword="true"/> if <paramref name="predicateArgument"/> is a recognized white-space predicate; otherwise, <see langword="false"/>.</returns>
    private static bool IsWhiteSpacePredicate(ExpressionSyntax predicateArgument)
    {
        var predicate = SyntaxHelpers.Unwrap(predicateArgument);

        if (IsCharIsWhiteSpaceMemberAccess(predicate))
        {
            return true;
        }

        return predicate is SimpleLambdaExpressionSyntax { ExpressionBody: { } body } lambda
            && SyntaxHelpers.Unwrap(body)
                is InvocationExpressionSyntax { Expression: var callee, ArgumentList.Arguments.Count: 1 } call
            && IsCharIsWhiteSpaceMemberAccess(callee)
            && SyntaxHelpers.Unwrap(call.ArgumentList.Arguments[0].Expression) is IdentifierNameSyntax paramRef
            && paramRef.Identifier.Text == lambda.Parameter.Identifier.Text;
    }

    /// <summary>
    /// Determines whether the receiver being validated is a <see cref="string"/>. The <c>Any</c> shape recognized by
    /// <see cref="TryGetContainsWhiteSpaceTarget"/> matches purely syntactically and also fires on any
    /// <c>IEnumerable&lt;char&gt;</c> receiver (e.g. <c>char[]</c> or <c>List&lt;char&gt;</c>), for which
    /// <c>ArgumentException.ThrowIfContainsWhiteSpace(string?)</c> is not an applicable replacement.
    /// </summary>
    /// <param name="receiver">The expression being validated, as resolved by <see cref="TryGetContainsWhiteSpaceTarget"/>.</param>
    /// <param name="semanticModel">The semantic model used to resolve the receiver's type.</param>
    /// <param name="cancellationToken">The token used to cancel semantic-model lookups.</param>
    /// <returns><see langword="true"/> if <paramref name="receiver"/> is of type <see cref="string"/>; otherwise, <see langword="false"/>.</returns>
    private static bool IsStringReceiver(
        ExpressionSyntax receiver,
        SemanticModel semanticModel,
        CancellationToken cancellationToken
    ) => semanticModel.GetTypeInfo(receiver, cancellationToken).Type?.SpecialType == SpecialType.System_String;

    /// <summary>Determines whether an invocation resolves to the named method on <see cref="System.Linq.Enumerable"/> (e.g. <c>Any</c>, <c>Count</c> or <c>Where</c>), rather than some other method or extension of the same name.</summary>
    /// <param name="invocation">The invocation expression to inspect.</param>
    /// <param name="expectedMethodName">The expected method name, e.g. <c>"Any"</c>, <c>"Count"</c> or <c>"Where"</c>.</param>
    /// <param name="semanticModel">The semantic model used to resolve the invoked method.</param>
    /// <param name="cancellationToken">The token used to cancel semantic-model lookups.</param>
    /// <returns><see langword="true"/> if <paramref name="invocation"/> invokes <c>System.Linq.Enumerable.{expectedMethodName}</c>; otherwise, <see langword="false"/>.</returns>
    private static bool IsLinqEnumerableMethod(
        InvocationExpressionSyntax invocation,
        string expectedMethodName,
        SemanticModel semanticModel,
        CancellationToken cancellationToken
    ) =>
        semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol
            is IMethodSymbol { ContainingType: { } containingType } method
        && method.Name == expectedMethodName
        && containingType.ToDisplayString() == "System.Linq.Enumerable";

    /// <summary>Determines whether an expression is a <c>char.IsWhiteSpace</c> member access, either via the <see cref="char"/> keyword or the <c>Char</c> identifier.</summary>
    /// <param name="expression">The expression to test.</param>
    /// <returns><see langword="true"/> if <paramref name="expression"/> is <c>char.IsWhiteSpace</c> or <c>Char.IsWhiteSpace</c>; otherwise, <see langword="false"/>.</returns>
    private static bool IsCharIsWhiteSpaceMemberAccess(ExpressionSyntax expression)
    {
        if (
            SyntaxHelpers.Unwrap(expression)
            is not MemberAccessExpressionSyntax { Expression: var typeReference, Name.Identifier.Text: "IsWhiteSpace" }
        )
        {
            return false;
        }

        return SyntaxHelpers.Unwrap(typeReference) switch
        {
            PredefinedTypeSyntax predefinedType => predefinedType.Keyword.IsKind(SyntaxKind.CharKeyword),
            IdentifierNameSyntax { Identifier.Text: "Char" } => true,
            _ => false,
        };
    }
}
