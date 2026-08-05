namespace NetEvolve.Arguments.Analyser;

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Ensures the <c>using System;</c> directive required to resolve the <c>System</c> throw-helper
/// extension members is present in a fixed document. The code fix providers in this project emit
/// unqualified receivers (e.g. <c>ArgumentNullException.ThrowIfNull(...)</c>); on frameworks where the
/// call binds to the polyfilled extension members in <c>NetEvolve.Arguments</c>, the extension block
/// lives in namespace <c>System</c>, so the containing namespace must be imported for the call to
/// resolve, even though the diagnostic itself can fire on a fully-qualified throw expression that
/// required no such <see langword="using"/> directive.
/// </summary>
internal static class UsingDirectiveInserter
{
    private const string RequiredNamespace = "System";

    /// <summary>
    /// Inserts a <c>using System;</c> directive into <paramref name="root"/> unless one is already present,
    /// respecting whether the file uses a file-scoped or block-scoped namespace declaration (or none at all).
    /// </summary>
    /// <param name="root">The compilation unit to update.</param>
    /// <returns>The compilation unit with a <c>using System;</c> directive present.</returns>
    internal static CompilationUnitSyntax EnsureSystemUsingDirective(CompilationUnitSyntax root)
    {
        if (HasSystemUsing(root.Usings))
        {
            return root;
        }

        if (root.Members.Count == 1 && root.Members[0] is FileScopedNamespaceDeclarationSyntax fileScopedNamespace)
        {
            if (HasSystemUsing(fileScopedNamespace.Usings))
            {
                return root;
            }

            var updatedNamespace = fileScopedNamespace.WithUsings(
                fileScopedNamespace.Usings.Add(CreateSystemUsingDirective())
            );

            return root.WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(updatedNamespace));
        }

        if (root.Members.Count == 1 && root.Members[0] is NamespaceDeclarationSyntax blockNamespace)
        {
            if (HasSystemUsing(blockNamespace.Usings))
            {
                return root;
            }

            var updatedNamespace = blockNamespace.WithUsings(blockNamespace.Usings.Add(CreateSystemUsingDirective()));

            return root.WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(updatedNamespace));
        }

        return root.WithUsings(root.Usings.Add(CreateSystemUsingDirective()));
    }

    private static bool HasSystemUsing(SyntaxList<UsingDirectiveSyntax> usings) =>
        usings.Any(usingDirective =>
            usingDirective.Alias is null && usingDirective.Name?.ToString() == RequiredNamespace
        );

    private static UsingDirectiveSyntax CreateSystemUsingDirective() =>
        SyntaxFactory
            .UsingDirective(SyntaxFactory.IdentifierName(RequiredNamespace))
            .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
}
