namespace NetEvolve.Arguments.Analyser;

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>Reports <c>string.IsNullOrEmpty</c>/<c>IsNullOrWhiteSpace</c>-then-throw patterns that can be replaced by an <c>ArgumentException</c> throw-helper.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ThrowIfNullOrEmptyAnalyzer : DiagnosticAnalyzer
{
    private const string ArgumentExceptionMetadataName = "System.ArgumentException";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.ThrowIfNullOrEmpty);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        // ArgumentException.ThrowIfNullOrEmpty/ThrowIfNullOrWhiteSpace exist on the BCL since .NET 8;
        // where they do, the built-in CA1511 analyzer already covers this pattern, so stay silent.
        if (SyntaxHelpers.HasBuiltInMember(context.Compilation, ArgumentExceptionMetadataName, "ThrowIfNullOrEmpty"))
        {
            return;
        }

        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.IfStatement);
    }

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;

        if (ifStatement.Else is not null)
        {
            return;
        }

        if (!TryGetStringCheck(ifStatement.Condition, out var argument, out var helperName) || argument is null)
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
                DiagnosticDescriptors.ThrowIfNullOrEmpty,
                ifStatement.GetLocation(),
                helperName,
                argument.ToString()
            )
        );
    }

    internal static bool TryGetStringCheck(
        ExpressionSyntax condition,
        out ExpressionSyntax? argument,
        out string? helperName
    )
    {
        condition = SyntaxHelpers.Unwrap(condition);
        argument = null;
        helperName = null;

        if (
            condition
                is not InvocationExpressionSyntax
                {
                    Expression: MemberAccessExpressionSyntax
                    {
                        Expression: var typeReference,
                        Name.Identifier.Text: var methodName
                    },
                    ArgumentList.Arguments.Count: 1,
                } invocation
            || !IsStringTypeReference(typeReference)
        )
        {
            return false;
        }

        var target = invocation.ArgumentList.Arguments[0].Expression;

        switch (methodName)
        {
            case "IsNullOrEmpty":
                helperName = "ThrowIfNullOrEmpty";
                break;
            case "IsNullOrWhiteSpace":
                helperName = "ThrowIfNullOrWhiteSpace";
                break;
            default:
                return false;
        }

        argument = target;
        return true;
    }

    private static bool IsStringTypeReference(ExpressionSyntax expression) =>
        SyntaxHelpers.Unwrap(expression) switch
        {
            PredefinedTypeSyntax predefinedType => predefinedType.Keyword.IsKind(SyntaxKind.StringKeyword),
            IdentifierNameSyntax { Identifier.Text: "String" } => true,
            _ => false,
        };
}
