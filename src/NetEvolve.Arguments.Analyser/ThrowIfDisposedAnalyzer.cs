namespace NetEvolve.Arguments.Analyser;

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>Reports disposed-check-then-throw patterns that can be replaced by <c>ObjectDisposedException.ThrowIf</c>.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ThrowIfDisposedAnalyzer : DiagnosticAnalyzer
{
    private const string ObjectDisposedExceptionMetadataName = "System.ObjectDisposedException";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.ThrowIfDisposed);

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
        // ObjectDisposedException.ThrowIf exists on the BCL since .NET 7; where it does, the
        // built-in CA1513 analyzer already covers this pattern, so stay silent to avoid duplicates.
        if (SyntaxHelpers.HasBuiltInMember(context.Compilation, ObjectDisposedExceptionMetadataName, "ThrowIf"))
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

        var enclosingSymbol = context.SemanticModel.GetEnclosingSymbol(
            ifStatement.SpanStart,
            context.CancellationToken
        );

        if (enclosingSymbol is null || enclosingSymbol.IsStatic)
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
                ObjectDisposedExceptionMetadataName,
                context.CancellationToken
            )
        )
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticDescriptors.ThrowIfDisposed,
                ifStatement.GetLocation(),
                ifStatement.Condition.ToString()
            )
        );
    }
}
