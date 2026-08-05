namespace NetEvolve.Arguments.Analyser;

using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>Reports Guid.Empty-check-then-throw patterns that can be replaced by <c>ArgumentException.ThrowIfEmptyGuid</c>.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ThrowIfEmptyGuidAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The fully-qualified metadata name of <see cref="ArgumentException"/>.</summary>
    private const string ArgumentExceptionMetadataName = "System.ArgumentException";

    /// <summary>The fully-qualified metadata name of <see cref="Guid"/>.</summary>
    private const string GuidMetadataName = "System.Guid";

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

    /// <summary>Analyzes an <see langword="if"/> statement and reports NEA0009 when it is a <c>Guid.Empty</c>-check-then-throw of <see cref="ArgumentException"/>.</summary>
    /// <param name="context">The syntax-node analysis context for the <see langword="if"/> statement being visited.</param>
    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;

        if (
            !TryGetEmptyGuidCheckedExpression(
                ifStatement.Condition,
                context.SemanticModel,
                context.CancellationToken,
                out var argument
            ) || argument is null
        )
        {
            return;
        }

        var guidType = context.SemanticModel.Compilation.GetTypeByMetadataName(GuidMetadataName);
        var argumentType = context.SemanticModel.GetTypeInfo(argument, context.CancellationToken).Type;

        if (guidType is null || !SymbolEqualityComparer.Default.Equals(argumentType, guidType))
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

        if (!SyntaxHelpers.IsSingleParamNameArgument(argument, objectCreation.ArgumentList))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(DiagnosticDescriptors.ThrowIfEmptyGuid, ifStatement.GetLocation(), argument.ToString())
        );
    }

    /// <summary>Recognizes <c>arg.Equals(Guid.Empty)</c> and <c>arg == Guid.Empty</c>/<c>Guid.Empty == arg</c>.</summary>
    /// <param name="condition">The <see langword="if"/> statement's condition expression.</param>
    /// <param name="semanticModel">The semantic model used to resolve <c>Guid.Empty</c>.</param>
    /// <param name="cancellationToken">The token used to cancel semantic-model lookups.</param>
    /// <param name="argument">When this method returns <see langword="true"/>, the expression being checked; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="condition"/> is a recognized <c>Guid.Empty</c>-check shape; otherwise, <see langword="false"/>.</returns>
    internal static bool TryGetEmptyGuidCheckedExpression(
        ExpressionSyntax condition,
        SemanticModel semanticModel,
        CancellationToken cancellationToken,
        out ExpressionSyntax? argument
    )
    {
        condition = SyntaxHelpers.Unwrap(condition);
        argument = null;

        switch (condition)
        {
            case InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "Equals" } access,
                ArgumentList.Arguments.Count: 1,
            } invocation
                when IsGuidEmpty(invocation.ArgumentList.Arguments[0].Expression, semanticModel, cancellationToken):
                argument = access.Expression;
                return true;

            case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.EqualsExpression):
                if (IsGuidEmpty(binary.Right, semanticModel, cancellationToken))
                {
                    argument = binary.Left;
                    return true;
                }

                if (IsGuidEmpty(binary.Left, semanticModel, cancellationToken))
                {
                    argument = binary.Right;
                    return true;
                }

                break;
        }

        return false;
    }

    /// <summary>Determines whether an expression is a reference to the real <see cref="Guid.Empty"/> field.</summary>
    /// <param name="expression">The expression to test.</param>
    /// <param name="semanticModel">The semantic model used to resolve the expression's symbol.</param>
    /// <param name="cancellationToken">The token used to cancel semantic-model lookups.</param>
    /// <returns><see langword="true"/> if <paramref name="expression"/> resolves to <see cref="Guid.Empty"/>; otherwise, <see langword="false"/>.</returns>
    private static bool IsGuidEmpty(
        ExpressionSyntax expression,
        SemanticModel semanticModel,
        CancellationToken cancellationToken
    )
    {
        if (
            SyntaxHelpers.Unwrap(expression)
            is not MemberAccessExpressionSyntax { Name.Identifier.Text: "Empty" } memberAccess
        )
        {
            return false;
        }

        var symbol = semanticModel.GetSymbolInfo(memberAccess, cancellationToken).Symbol;
        var guidType = semanticModel.Compilation.GetTypeByMetadataName(GuidMetadataName);

        return guidType is not null
            && symbol is IFieldSymbol { Name: "Empty" } field
            && SymbolEqualityComparer.Default.Equals(field.ContainingType, guidType);
    }
}
