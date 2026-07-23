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

/// <summary>Replaces a collection-count-comparison-then-throw pattern with the matching <c>ArgumentException</c> throw-helper call.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ThrowIfCountCodeFixProvider))]
[Shared]
public sealed class ThrowIfCountCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.ThrowIfCount.Id);

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
            || !ThrowIfCountAnalyzer.TryGetCountComparison(ifStatement.Condition, out var comparison)
            || comparison is null
        )
        {
            return;
        }

        var title = $"Use ArgumentException.{comparison.Value.HelperName}";

        context.RegisterCodeFix(
            CodeAction.Create(
                title,
                cancellationToken => ApplyFixAsync(context.Document, ifStatement, cancellationToken),
                equivalenceKey: title
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
            || !ThrowIfCountAnalyzer.TryGetCountComparison(ifStatement.Condition, out var comparison)
            || comparison is null
        )
        {
            return document;
        }

        var value = comparison.Value;
        var arguments = value.OtherExpression2 is not null
            ? new[] { value.ValueExpression, value.OtherExpression!, value.OtherExpression2 }
            : new[] { value.ValueExpression, value.OtherExpression! };

        var invocation = SyntaxFactory
            .ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("ArgumentException"),
                        SyntaxFactory.IdentifierName(value.HelperName)
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
