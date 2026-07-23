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
using Microsoft.CodeAnalysis.Formatting;

/// <summary>Replaces a disposed-check-then-throw pattern with a call to <c>ObjectDisposedException.ThrowIf</c>.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ThrowIfDisposedCodeFixProvider))]
[Shared]
public sealed class ThrowIfDisposedCodeFixProvider : CodeFixProvider
{
    /// <summary>The display title shown for this fix in the lightbulb/quick-actions menu.</summary>
    private const string Title = "Use ObjectDisposedException.ThrowIf";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.ThrowIfDisposed.Id);

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
        var ifStatement = root.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<IfStatementSyntax>();

        if (ifStatement is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                Title,
                cancellationToken => ApplyFixAsync(context.Document, ifStatement, cancellationToken),
                equivalenceKey: Title
            ),
            diagnostic
        );
    }

    /// <summary>Rewrites the matched <c>if</c> statement into a single <c>ObjectDisposedException.ThrowIf(condition, this);</c> call.</summary>
    /// <param name="document">The document containing the diagnostic.</param>
    /// <param name="ifStatement">The <c>if</c> statement to replace.</param>
    /// <param name="cancellationToken">The token used to cancel the fix.</param>
    /// <returns>The updated document, or the original document if the syntax tree can no longer be retrieved.</returns>
    private static async Task<Document> ApplyFixAsync(
        Document document,
        IfStatementSyntax ifStatement,
        CancellationToken cancellationToken
    )
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root is null)
        {
            return document;
        }

        var invocation = SyntaxFactory
            .ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("ObjectDisposedException"),
                        SyntaxFactory.IdentifierName("ThrowIf")
                    ),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(
                            new[]
                            {
                                SyntaxFactory.Argument(ifStatement.Condition.WithoutTrivia()),
                                SyntaxFactory.Argument(SyntaxFactory.ThisExpression()),
                            }
                        )
                    )
                )
            )
            .WithTriviaFrom(ifStatement)
            .WithAdditionalAnnotations(Formatter.Annotation);

        var newRoot = root.ReplaceNode(ifStatement, invocation);

        return document.WithSyntaxRoot(newRoot);
    }
}
