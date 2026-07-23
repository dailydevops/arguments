namespace NetEvolve.Arguments.Analyser;

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>Reports Guid.Empty-check-then-throw patterns that can be replaced by <c>ArgumentException.ThrowIfEmptyGuid</c>.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ThrowIfEmptyGuidAnalyzer : DiagnosticAnalyzer
{
    private const string ArgumentExceptionMetadataName = "System.ArgumentException";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.ThrowIfEmptyGuid);

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

        if (!TryGetEmptyGuidCheckedExpression(ifStatement.Condition, out var argument) || argument is null)
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

        if (objectCreation.ArgumentList is null)
        {
            return;
        }

        if (!SyntaxHelpers.IsSingleParamNameArgument(argument, objectCreation.ArgumentList))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(DiagnosticDescriptors.ThrowIfEmptyGuid, ifStatement.GetLocation(), argument.ToString())
        );
    }

    internal static bool TryGetEmptyGuidCheckedExpression(ExpressionSyntax condition, out ExpressionSyntax? argument)
    {
        condition = SyntaxHelpers.Unwrap(condition);
        argument = null;

        switch (condition)
        {
            case InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "Equals" } access,
                ArgumentList.Arguments.Count: 1,
            } invocation when IsGuidEmpty(invocation.ArgumentList.Arguments[0].Expression):
                argument = access.Expression;
                return true;

            case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.EqualsExpression):
                if (IsGuidEmpty(binary.Right))
                {
                    argument = binary.Left;
                    return true;
                }

                if (IsGuidEmpty(binary.Left))
                {
                    argument = binary.Right;
                    return true;
                }

                break;
        }

        return false;
    }

    private static bool IsGuidEmpty(ExpressionSyntax expression) =>
        SyntaxHelpers.Unwrap(expression)
            is MemberAccessExpressionSyntax
            {
                Expression: IdentifierNameSyntax { Identifier.Text: "Guid" },
                Name.Identifier.Text: "Empty",
            };
}
