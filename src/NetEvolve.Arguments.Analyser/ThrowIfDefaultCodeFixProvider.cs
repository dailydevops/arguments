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

/// <summary>Replaces a default-value-check-then-throw pattern with a call to <c>ArgumentException.ThrowIfDefault</c>.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ThrowIfDefaultCodeFixProvider))]
[Shared]
public sealed class ThrowIfDefaultCodeFixProvider : CodeFixProvider
{
    /// <summary>The display title shown for this fix in the lightbulb/quick-actions menu.</summary>
    private const string Title = "Use ArgumentException.ThrowIfDefault";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.ThrowIfDefault.Id);

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

    /// <summary>Rewrites the matched <c>if</c> statement into a single call to <c>ArgumentException.ThrowIfDefault</c>.</summary>
    /// <param name="document">The document containing the diagnostic.</param>
    /// <param name="ifStatement">The <c>if</c> statement to replace.</param>
    /// <param name="cancellationToken">The token used to cancel the fix.</param>
    /// <returns>The updated document, or the original document if the pattern can no longer be matched.</returns>
    private static async Task<Document> ApplyFixAsync(
        Document document,
        IfStatementSyntax ifStatement,
        CancellationToken cancellationToken
    )
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (
            root is null
            || !ThrowIfDefaultAnalyzer.TryGetDefaultCheckedExpression(ifStatement.Condition, out var argument)
            || argument is null
        )
        {
            return document;
        }

        var invocation = SyntaxFactory
            .ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("ArgumentException"),
                        SyntaxFactory.IdentifierName("ThrowIfDefault")
                    ),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(argument.WithoutTrivia()))
                    )
                )
            )
            .WithTriviaFrom(ifStatement)
            .WithAdditionalAnnotations(Formatter.Annotation);

        var newRoot = root.ReplaceNode(ifStatement, invocation);

        return document.WithSyntaxRoot(newRoot);
    }
}
