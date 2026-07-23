namespace NetEvolve.Arguments.Analyser;

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;

/// <summary>Replaces a null-check-then-throw pattern with a call to <c>ArgumentNullException.ThrowIfNull</c>.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ThrowIfNullCodeFixProvider))]
[Shared]
public sealed class ThrowIfNullCodeFixProvider : CodeFixProvider
{
    private const string Title = "Use ArgumentNullException.ThrowIfNull";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.ThrowIfNull.Id);

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var node = root.FindNode(diagnostic.Location.SourceSpan);

        if (node.FirstAncestorOrSelf<IfStatementSyntax>() is { } ifStatement)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    cancellationToken => ApplyIfStatementFixAsync(context.Document, ifStatement, cancellationToken),
                    equivalenceKey: Title
                ),
                diagnostic
            );
            return;
        }

        if (node is BinaryExpressionSyntax { RawKind: (int)SyntaxKind.CoalesceExpression } coalesce)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    cancellationToken => ApplyCoalesceFixAsync(context.Document, coalesce, cancellationToken),
                    equivalenceKey: Title
                ),
                diagnostic
            );
        }
    }

    private static async Task<Document> ApplyIfStatementFixAsync(
        Document document,
        IfStatementSyntax ifStatement,
        CancellationToken cancellationToken
    )
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (
            root is null
            || !SyntaxHelpers.TryGetNullCheckedExpression(ifStatement.Condition, out var argument)
            || argument is null
        )
        {
            return document;
        }

        var throwIfNullInvocation = SyntaxFactory
            .ExpressionStatement(CreateThrowIfNullInvocation(argument))
            .WithTriviaFrom(ifStatement)
            .WithAdditionalAnnotations(Formatter.Annotation);

        var newRoot = root.ReplaceNode(ifStatement, throwIfNullInvocation);

        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> ApplyCoalesceFixAsync(
        Document document,
        BinaryExpressionSyntax coalesce,
        CancellationToken cancellationToken
    )
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        var containingStatement = coalesce.FirstAncestorOrSelf<StatementSyntax>();

        if (
            semanticModel is null
            || containingStatement is null
            || !SyntaxHelpers.TryGetCoalesceNullCheck(semanticModel, coalesce, cancellationToken, out var argument)
            || argument is null
        )
        {
            return document;
        }

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

        var throwIfNullStatement = SyntaxFactory
            .ExpressionStatement(CreateThrowIfNullInvocation(argument))
            .WithLeadingTrivia(containingStatement.GetLeadingTrivia())
            .WithAdditionalAnnotations(Formatter.Annotation);

        editor.InsertBefore(containingStatement, throwIfNullStatement);
        editor.ReplaceNode(coalesce, argument.WithoutTrivia());

        return editor.GetChangedDocument();
    }

    private static InvocationExpressionSyntax CreateThrowIfNullInvocation(ExpressionSyntax argument) =>
        SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName("ArgumentNullException"),
                SyntaxFactory.IdentifierName("ThrowIfNull")
            ),
            SyntaxFactory.ArgumentList(
                SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(argument.WithoutTrivia()))
            )
        );
}
