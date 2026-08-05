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
    /// <summary>The fully-qualified metadata name of <see cref="ObjectDisposedException"/>.</summary>
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

    /// <summary>Registers the syntax-node action for this rule, unless the compilation's BCL already exposes <c>ObjectDisposedException.ThrowIf</c>.</summary>
    /// <param name="context">The compilation-start context supplied by the Roslyn analyzer driver.</param>
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

    /// <summary>
    /// Analyzes an <see langword="if"/> statement and reports NEA0005 when it is a disposed-check-then-throw of
    /// <see cref="ObjectDisposedException"/> inside an instance member (the fix requires <see langword="this"/>).
    /// </summary>
    /// <param name="context">The syntax-node analysis context for the <see langword="if"/> statement being visited.</param>
    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;

        if (
            !SyntaxHelpers.TryGetThrownException(
                ifStatement,
                context.SemanticModel,
                ObjectDisposedExceptionMetadataName,
                context.CancellationToken,
                out var objectCreation
            ) || objectCreation!.ArgumentList is null
        )
        {
            return;
        }

        var enclosingSymbol = context.SemanticModel.GetEnclosingSymbol(
            ifStatement.SpanStart,
            context.CancellationToken
        );

        if (enclosingSymbol is null)
        {
            return;
        }

        // Walk up through local functions and lambdas: `IsStatic` on the innermost enclosing
        // symbol only reflects the `static` modifier written on that local function/lambda
        // itself, not on the member that contains it. The fix requires `this`, so bail out if
        // any enclosing scope - the local function/lambda chain or the member containing it -
        // is static.
        var symbol = enclosingSymbol;
        while (symbol is not null)
        {
            if (symbol.IsStatic)
            {
                return;
            }

            if (symbol is not IMethodSymbol { MethodKind: MethodKind.LocalFunction or MethodKind.AnonymousFunction })
            {
                break;
            }

            symbol = symbol.ContainingSymbol;
        }

        // ObjectDisposedException.ThrowIf(condition, instance) always derives the object name from the
        // enclosing runtime type; it has no parameter for a message. The single-argument constructor
        // (objectName) is already lossy in the same way and is accepted for parity with existing behavior,
        // but the two-argument constructor (objectName, message) carries a message that would silently
        // disappear, so it is rejected here.
        if (objectCreation.ArgumentList.Arguments.Count > 1)
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
