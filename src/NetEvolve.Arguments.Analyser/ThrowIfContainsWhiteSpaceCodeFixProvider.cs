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

/// <summary>Replaces a white-space-check-then-throw pattern with a call to <c>ArgumentException.ThrowIfContainsWhiteSpace</c>.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ThrowIfContainsWhiteSpaceCodeFixProvider))]
[Shared]
public sealed class ThrowIfContainsWhiteSpaceCodeFixProvider : CodeFixProvider
{
    private const string Title = "Use ArgumentException.ThrowIfContainsWhiteSpace";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.ThrowIfContainsWhiteSpace.Id);

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

    private static async Task<Document> ApplyFixAsync(
        Document document,
        IfStatementSyntax ifStatement,
        CancellationToken cancellationToken
    )
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (
            root is null
            || !ThrowIfContainsWhiteSpaceAnalyzer.TryGetContainsWhiteSpaceTarget(
                ifStatement.Condition,
                out var argument
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
                        SyntaxFactory.IdentifierName("ThrowIfContainsWhiteSpace")
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
