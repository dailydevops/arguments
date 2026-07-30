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
    /// <summary>The fully-qualified metadata name of <see cref="ArgumentException"/>.</summary>
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

    /// <summary>
    /// Probes the compilation's BCL for both <c>ArgumentException.ThrowIfNullOrEmpty</c> and <c>ArgumentException.ThrowIfNullOrWhiteSpace</c>,
    /// and registers the syntax-node action unless both are already present.
    /// </summary>
    /// <param name="context">The compilation-start context supplied by the Roslyn analyzer driver.</param>
    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        // The BCL exposes ThrowIfNullOrEmpty since .NET 7 and ThrowIfNullOrWhiteSpace since .NET 8; where a
        // given helper already exists, the built-in CA1511 analyzer already covers that shape, so this rule
        // must stay silent for it to avoid duplicates. The two helpers can be independently available (e.g.
        // net7.0's System.Runtime declares only ThrowIfNullOrEmpty), so each is probed separately and the
        // per-branch suppression happens in Analyze; only skip registration entirely when both already exist.
        var hasThrowIfNullOrEmpty = SyntaxHelpers.HasBuiltInMember(
            context.Compilation,
            ArgumentExceptionMetadataName,
            "ThrowIfNullOrEmpty"
        );
        var hasThrowIfNullOrWhiteSpace = SyntaxHelpers.HasBuiltInMember(
            context.Compilation,
            ArgumentExceptionMetadataName,
            "ThrowIfNullOrWhiteSpace"
        );

        if (hasThrowIfNullOrEmpty && hasThrowIfNullOrWhiteSpace)
        {
            return;
        }

        context.RegisterSyntaxNodeAction(
            nodeContext => Analyze(nodeContext, hasThrowIfNullOrEmpty, hasThrowIfNullOrWhiteSpace),
            SyntaxKind.IfStatement
        );
    }

    /// <summary>Analyzes an <c>if</c> statement and reports NEA0002 when it is a <c>string.IsNullOrEmpty</c>/<c>IsNullOrWhiteSpace</c>-then-throw of <see cref="ArgumentException"/>.</summary>
    /// <param name="context">The syntax-node analysis context for the <c>if</c> statement being visited.</param>
    /// <param name="hasThrowIfNullOrEmpty">Whether the compilation's BCL already exposes <c>ArgumentException.ThrowIfNullOrEmpty</c>.</param>
    /// <param name="hasThrowIfNullOrWhiteSpace">Whether the compilation's BCL already exposes <c>ArgumentException.ThrowIfNullOrWhiteSpace</c>.</param>
    private static void Analyze(
        SyntaxNodeAnalysisContext context,
        bool hasThrowIfNullOrEmpty,
        bool hasThrowIfNullOrWhiteSpace
    )
    {
        var ifStatement = (IfStatementSyntax)context.Node;

        if (!TryGetStringCheck(ifStatement.Condition, out var argument, out var helperName) || argument is null)
        {
            return;
        }

        if (
            (helperName is "ThrowIfNullOrEmpty" && hasThrowIfNullOrEmpty)
            || (helperName is "ThrowIfNullOrWhiteSpace" && hasThrowIfNullOrWhiteSpace)
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

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticDescriptors.ThrowIfNullOrEmpty,
                ifStatement.GetLocation(),
                helperName,
                argument.ToString()
            )
        );
    }

    /// <summary>Recognizes <c>string.IsNullOrEmpty(arg)</c>/<c>IsNullOrWhiteSpace(arg)</c> and reports the matching throw-helper member name and the checked argument.</summary>
    /// <param name="condition">The <c>if</c> statement's condition expression.</param>
    /// <param name="argument">When this method returns <see langword="true"/>, the string argument being checked; otherwise, <see langword="null"/>.</param>
    /// <param name="helperName">When this method returns <see langword="true"/>, the matching <see cref="ArgumentException"/> throw-helper member name; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="condition"/> is a recognized shape; otherwise, <see langword="false"/>.</returns>
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

    /// <summary>Determines whether an expression refers to the <see cref="string"/> type, either via the <c>string</c> keyword or the <c>String</c> identifier.</summary>
    /// <param name="expression">The invocation target's qualifier expression to test.</param>
    /// <returns><see langword="true"/> if <paramref name="expression"/> refers to <see cref="string"/>; otherwise, <see langword="false"/>.</returns>
    private static bool IsStringTypeReference(ExpressionSyntax expression) =>
        SyntaxHelpers.Unwrap(expression) switch
        {
            PredefinedTypeSyntax predefinedType => predefinedType.Keyword.IsKind(SyntaxKind.StringKeyword),
            IdentifierNameSyntax { Identifier.Text: "String" } => true,
            _ => false,
        };
}
