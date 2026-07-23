namespace NetEvolve.Arguments.Analyser;

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>Reports null-check-then-throw patterns that can be replaced by <c>ArgumentNullException.ThrowIfNull</c>.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ThrowIfNullAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The fully-qualified metadata name of <see cref="ArgumentNullException"/>.</summary>
    private const string ArgumentNullExceptionMetadataName = "System.ArgumentNullException";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.ThrowIfNull);

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

    /// <summary>Registers the syntax-node actions for this rule, unless the compilation's BCL already exposes <c>ArgumentNullException.ThrowIfNull</c>.</summary>
    /// <param name="context">The compilation-start context supplied by the Roslyn analyzer driver.</param>
    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        // ArgumentNullException.ThrowIfNull exists on the BCL since .NET 6; where it does, the
        // built-in CA1510 analyzer already covers this pattern, so stay silent to avoid duplicates.
        if (SyntaxHelpers.HasBuiltInMember(context.Compilation, ArgumentNullExceptionMetadataName, "ThrowIfNull"))
        {
            return;
        }

        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.IfStatement);
        context.RegisterSyntaxNodeAction(AnalyzeCoalesce, SyntaxKind.CoalesceExpression);
    }

    /// <summary>Analyzes a <c>??</c> expression and reports NEA0001 when it is a null-coalescing throw of <see cref="ArgumentNullException"/>.</summary>
    /// <param name="context">The syntax-node analysis context for the coalesce expression being visited.</param>
    private static void AnalyzeCoalesce(SyntaxNodeAnalysisContext context)
    {
        var binary = (BinaryExpressionSyntax)context.Node;

        if (
            !SyntaxHelpers.TryGetCoalesceNullCheck(
                context.SemanticModel,
                binary,
                context.CancellationToken,
                out var argument
            ) || argument is null
        )
        {
            return;
        }

        if (binary.FirstAncestorOrSelf<StatementSyntax>() is null)
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(DiagnosticDescriptors.ThrowIfNull, binary.GetLocation(), argument.ToString())
        );
    }

    /// <summary>Analyzes an <c>if</c> statement and reports NEA0001 when it is a null-check-then-throw of <see cref="ArgumentNullException"/>.</summary>
    /// <param name="context">The syntax-node analysis context for the <c>if</c> statement being visited.</param>
    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;

        if (!SyntaxHelpers.TryGetNullCheckedExpression(ifStatement.Condition, out var argument) || argument is null)
        {
            return;
        }

        if (
            !SyntaxHelpers.TryGetThrownException(
                ifStatement,
                context.SemanticModel,
                ArgumentNullExceptionMetadataName,
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
            Diagnostic.Create(DiagnosticDescriptors.ThrowIfNull, ifStatement.GetLocation(), argument.ToString())
        );
    }
}
