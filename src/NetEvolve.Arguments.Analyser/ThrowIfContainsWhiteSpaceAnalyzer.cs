namespace NetEvolve.Arguments.Analyser;

using System;
using System.Collections.Immutable;
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

    /// <summary>Analyzes an <c>if</c> statement and reports NEA0008 when it is a white-space-check-then-throw of <see cref="ArgumentException"/>.</summary>
    /// <param name="context">The syntax-node analysis context for the <c>if</c> statement being visited.</param>
    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;

        if (ifStatement.Else is not null)
        {
            return;
        }

        if (!TryGetContainsWhiteSpaceTarget(ifStatement.Condition, out var argument) || argument is null)
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

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticDescriptors.ThrowIfContainsWhiteSpace,
                ifStatement.GetLocation(),
                argument.ToString()
            )
        );
    }

    /// <summary>Recognizes <c>arg.Any(c => char.IsWhiteSpace(c))</c> and the method-group form <c>arg.Any(char.IsWhiteSpace)</c>.</summary>
    /// <param name="condition">The <c>if</c> statement's condition expression.</param>
    /// <param name="argument">When this method returns <see langword="true"/>, the string argument being checked; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="condition"/> is a recognized white-space-check shape; otherwise, <see langword="false"/>.</returns>
    internal static bool TryGetContainsWhiteSpaceTarget(ExpressionSyntax condition, out ExpressionSyntax? argument)
    {
        argument = null;
        condition = SyntaxHelpers.Unwrap(condition);

        if (
            condition
            is not InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "Any" } access,
                ArgumentList.Arguments.Count: 1,
            } invocation
        )
        {
            return false;
        }

        var predicate = SyntaxHelpers.Unwrap(invocation.ArgumentList.Arguments[0].Expression);

        if (IsCharIsWhiteSpaceMemberAccess(predicate))
        {
            argument = access.Expression;
            return true;
        }

        if (
            predicate is SimpleLambdaExpressionSyntax { ExpressionBody: { } body } lambda
            && SyntaxHelpers.Unwrap(body)
                is InvocationExpressionSyntax { Expression: var callee, ArgumentList.Arguments.Count: 1 } call
            && IsCharIsWhiteSpaceMemberAccess(callee)
            && SyntaxHelpers.Unwrap(call.ArgumentList.Arguments[0].Expression) is IdentifierNameSyntax paramRef
            && paramRef.Identifier.Text == lambda.Parameter.Identifier.Text
        )
        {
            argument = access.Expression;
            return true;
        }

        return false;
    }

    /// <summary>Determines whether an expression is a <c>char.IsWhiteSpace</c> member access, either via the <c>char</c> keyword or the <c>Char</c> identifier.</summary>
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
