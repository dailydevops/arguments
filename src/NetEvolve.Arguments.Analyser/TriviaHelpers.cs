namespace NetEvolve.Arguments.Analyser;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>Shared trivia-preservation helpers used by the code fix providers that replace an <see langword="if"/> statement.</summary>
internal static class TriviaHelpers
{
    /// <summary>
    /// Copies the leading and trailing trivia of <paramref name="ifStatement"/> onto <paramref name="replacement"/>,
    /// exactly like the <c>WithTriviaFrom</c> extension method, and additionally re-inserts any comment trivia attached
    /// to tokens INSIDE the <see langword="if"/> statement's subtree (for example, a comment on its own line right before the
    /// <see langword="throw"/> statement inside the block) that <c>WithTriviaFrom</c> alone would silently discard, since it
    /// only carries the leading trivia of the <see langword="if"/> keyword and the trailing trivia after the closing brace.
    /// </summary>
    /// <typeparam name="TNode">The type of the replacement syntax node.</typeparam>
    /// <param name="replacement">The node that is replacing <paramref name="ifStatement"/>.</param>
    /// <param name="ifStatement">The <see langword="if"/> statement being replaced.</param>
    /// <returns><paramref name="replacement"/> with the <see langword="if"/> statement's trivia and any interior comments preserved.</returns>
    public static TNode WithTriviaFromPreservingComments<TNode>(this TNode replacement, IfStatementSyntax ifStatement)
        where TNode : SyntaxNode
    {
        var withOuterTrivia = replacement.WithTriviaFrom(ifStatement);

        // These are the trivia lists WithTriviaFrom already carried over; anything found in them must not be
        // duplicated when we scan the whole subtree for comments below.
        var alreadyCarried = new HashSet<SyntaxTrivia>(
            ifStatement.GetLeadingTrivia().Concat(ifStatement.GetTrailingTrivia())
        );

        var interiorComments = ifStatement
            .DescendantTrivia()
            .Where(trivia =>
                (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                && !alreadyCarried.Contains(trivia)
            )
            .ToList();

        if (interiorComments.Count == 0)
        {
            return withOuterTrivia;
        }

        var preservedTrivia = interiorComments.SelectMany(comment =>
            new[] { comment, SyntaxFactory.ElasticCarriageReturnLineFeed }
        );

        // Append after whatever leading trivia was already carried over (e.g. a comment preceding the `if` itself),
        // so the interior comments keep appearing immediately before the replacement statement, matching their
        // original source position right before the `throw`.
        return withOuterTrivia.WithLeadingTrivia(withOuterTrivia.GetLeadingTrivia().Concat(preservedTrivia));
    }
}
