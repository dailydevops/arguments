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
