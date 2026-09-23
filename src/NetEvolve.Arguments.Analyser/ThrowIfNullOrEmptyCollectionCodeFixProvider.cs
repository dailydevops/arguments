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

/// <summary>Replaces a null-or-empty-collection-check-then-throw pattern with an <c>ArgumentException.ThrowIfNullOrEmpty</c> call.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ThrowIfNullOrEmptyCollectionCodeFixProvider))]
[Shared]
public sealed class ThrowIfNullOrEmptyCollectionCodeFixProvider : CodeFixProvider
{
    /// <summary>The title and equivalence key of the registered code action.</summary>
    private const string Title = "Use ArgumentException.ThrowIfNullOrEmpty";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.ThrowIfNullOrEmptyCollection.Id);

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

        if (
            ifStatement is null
            || !ThrowIfNullOrEmptyCollectionAnalyzer.TryGetCollectionCheck(ifStatement.Condition, out _, out _)
        )
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

    /// <summary>Rewrites the matched <see langword="if"/> statement into a single <c>ArgumentException.ThrowIfNullOrEmpty</c> call.</summary>
    /// <param name="document">The document containing the diagnostic.</param>
    /// <param name="ifStatement">The <see langword="if"/> statement to replace.</param>
    /// <param name="cancellationToken">The token used to cancel the fix.</param>
    /// <returns>The updated document, or the original document if the pattern can no longer be matched.</returns>
    private static async Task<Document> ApplyFixAsync(
        Document document,
        IfStatementSyntax ifStatement,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (
            root is null
            || !ThrowIfNullOrEmptyCollectionAnalyzer.TryGetCollectionCheck(
                ifStatement.Condition,
                out var argument,
                out _
            )
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
                        SyntaxFactory.IdentifierName("ThrowIfNullOrEmpty")
                    ),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(argument.WithoutTrivia()))
                    )
                )
            )
            .WithTriviaFromPreservingComments(ifStatement)
            .WithAdditionalAnnotations(Formatter.Annotation);

        var newRoot = root.ReplaceNode(ifStatement, invocation);

        if (newRoot is CompilationUnitSyntax compilationUnit)
        {
            newRoot = UsingDirectiveInserter.EnsureSystemUsingDirective(compilationUnit);
        }

        return document.WithSyntaxRoot(newRoot);
    }
}
