namespace NetEvolve.Arguments.Analyser;

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

/// <summary>Replaces a comparison-then-throw pattern with the matching <c>ArgumentOutOfRangeException</c> throw-helper call.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ThrowIfOutOfRangeCodeFixProvider))]
[Shared]
public sealed class ThrowIfOutOfRangeCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.ThrowIfOutOfRange.Id);

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
            || !ThrowIfOutOfRangeAnalyzer.TryGetComparison(ifStatement.Condition, out var comparison)
            || comparison is null
        )
        {
            return;
        }

        var title = $"Use ArgumentOutOfRangeException.{comparison.Value.HelperName}";

        context.RegisterCodeFix(
            CodeAction.Create(
                title,
                cancellationToken => ApplyFixAsync(context.Document, ifStatement, cancellationToken),
                equivalenceKey: title
            ),
            diagnostic
        );
    }

    /// <summary>Rewrites the matched <c>if</c> statement into a single call to the given <see cref="ArgumentOutOfRangeException"/> throw-helper.</summary>
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
            || !ThrowIfOutOfRangeAnalyzer.TryGetComparison(ifStatement.Condition, out var comparison)
            || comparison is null
        )
        {
            return document;
        }

        var value = comparison.Value;
        ExpressionSyntax[] arguments;

        if (value.OtherExpression2 is not null)
        {
            arguments = new[] { value.ValueExpression, value.OtherExpression!, value.OtherExpression2 };
        }
        else if (value.OtherExpression is not null)
        {
            arguments = new[] { value.ValueExpression, value.OtherExpression };
        }
        else
        {
            arguments = new[] { value.ValueExpression };
        }

        var invocation = SyntaxFactory
            .ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("ArgumentOutOfRangeException"),
                        SyntaxFactory.IdentifierName(comparison.Value.HelperName)
                    ),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(
                            arguments.Select(argument => SyntaxFactory.Argument(argument.WithoutTrivia()))
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
