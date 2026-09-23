namespace NetEvolve.Arguments.Analyser;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>Reports null-or-empty-collection-check-then-throw patterns that can be replaced by <c>ArgumentException.ThrowIfNullOrEmpty</c>.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ThrowIfNullOrEmptyCollectionAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The fully-qualified metadata name of <see cref="ArgumentException"/>.</summary>
    private const string ArgumentExceptionMetadataName = "System.ArgumentException";

    /// <summary>The fully-qualified metadata name of <see cref="ArgumentNullException"/>.</summary>
    /// <remarks>
    /// Also accepted as the thrown type, mirroring NEA0002: a null-or-empty check that throws
    /// <see cref="ArgumentNullException"/> for the empty-but-non-null case is itself a bug, and the throw-helper
    /// fix corrects both the maintainability concern and the wrong exception type.
    /// </remarks>
    private const string ArgumentNullExceptionMetadataName = "System.ArgumentNullException";

    /// <summary>The fully-qualified metadata name of the open generic <c>ICollection&lt;T&gt;</c> interface.</summary>
    private const string CollectionInterfaceMetadataName = "System.Collections.Generic.ICollection`1";

    /// <summary>The fully-qualified metadata name of the open generic <c>IReadOnlyCollection&lt;T&gt;</c> interface.</summary>
    private const string ReadOnlyCollectionInterfaceMetadataName = "System.Collections.Generic.IReadOnlyCollection`1";

    /// <summary>The fully-qualified metadata name of <see cref="System.Linq.Enumerable"/>, the declaring type of the LINQ <c>Any()</c>/<c>Count()</c> extension methods.</summary>
    private const string EnumerableTypeMetadataName = "System.Linq.Enumerable";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.ThrowIfNullOrEmptyCollection);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.IfStatement);
    }

    /// <summary>Analyzes an <see langword="if"/> statement and reports NEA0010 when it is a null-or-empty-collection-check-then-throw of <see cref="ArgumentException"/>.</summary>
    /// <param name="context">The syntax-node analysis context for the <see langword="if"/> statement being visited.</param>
    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;

        if (
            !TryGetCollectionCheck(ifStatement.Condition, out var argument, out var emptyCheck)
            || argument is null
            || emptyCheck is null
        )
        {
            return;
        }

        if (!IsSupportedEmptyCheck(emptyCheck, argument, context.SemanticModel, context.CancellationToken))
        {
            return;
        }

        if (
            !(
                SyntaxHelpers.TryGetThrownException(
                    ifStatement,
                    context.SemanticModel,
                    ArgumentExceptionMetadataName,
                    context.CancellationToken,
                    out var objectCreation
                )
                || SyntaxHelpers.TryGetThrownException(
                    ifStatement,
                    context.SemanticModel,
                    ArgumentNullExceptionMetadataName,
                    context.CancellationToken,
                    out objectCreation
                )
            ) || objectCreation!.ArgumentList is null
        )
        {
            return;
        }

        if (
            !ArgumentExceptionParamNameHelpers.IsSingleParamNameOrEmptyMessageArgument(
                argument,
                objectCreation.ArgumentList
            )
        )
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticDescriptors.ThrowIfNullOrEmptyCollection,
                ifStatement.GetLocation(),
                argument.ToString()
            )
        );
    }

    /// <summary>
    /// Recognizes <c>&lt;null-check&gt; || &lt;empty-check&gt;</c>, where the null check is any shape accepted by
    /// <see cref="SyntaxHelpers.TryGetNullCheckedExpression"/> and the empty check is <c>!arg.Any()</c>,
    /// <c>arg.Count == 0</c>, <c>arg.Count() == 0</c>, or <c>arg.Length == 0</c> (either operand order for
    /// <c>==</c>), and both sides refer to the same argument.
    /// </summary>
    /// <remarks>
    /// The null check must come first: the reversed order dereferences the argument before checking it for
    /// <see langword="null"/>, so it throws <see cref="NullReferenceException"/> and is not equivalent to the throw-helper.
    /// </remarks>
    /// <param name="condition">The <see langword="if"/> statement's condition expression.</param>
    /// <param name="argument">When this method returns <see langword="true"/>, the collection argument being checked; otherwise, <see langword="null"/>.</param>
    /// <param name="emptyCheck">When this method returns <see langword="true"/>, the <c>Any()</c>/<c>Count</c>/<c>Count()</c>/<c>Length</c> access of the empty check; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="condition"/> is a recognized shape; otherwise, <see langword="false"/>.</returns>
    internal static bool TryGetCollectionCheck(
        ExpressionSyntax condition,
        out ExpressionSyntax? argument,
        out ExpressionSyntax? emptyCheck
    )
    {
        condition = SyntaxHelpers.Unwrap(condition);
        argument = null;
        emptyCheck = null;

        if (condition is not BinaryExpressionSyntax { RawKind: (int)SyntaxKind.LogicalOrExpression } orExpression)
        {
            return false;
        }

        if (
            !SyntaxHelpers.TryGetNullCheckedExpression(orExpression.Left, out var nullChecked)
            || nullChecked is null
            || !TryGetEmptyCheck(orExpression.Right, out var emptyTarget, out var emptyAccess)
            || !SyntaxHelpers.AreEquivalent(SyntaxHelpers.Unwrap(nullChecked), SyntaxHelpers.Unwrap(emptyTarget!))
        )
        {
            return false;
        }

        argument = SyntaxHelpers.Unwrap(nullChecked);
        emptyCheck = emptyAccess;
        return true;
    }

    /// <summary>Recognizes <c>!arg.Any()</c>, <c>arg.Count == 0</c>, <c>arg.Count() == 0</c>, and <c>arg.Length == 0</c> (either operand order for <c>==</c>).</summary>
    /// <param name="expression">The right-hand side of the <c>||</c> condition.</param>
    /// <param name="target">When this method returns <see langword="true"/>, the expression whose emptiness is checked; otherwise, <see langword="null"/>.</param>
    /// <param name="access">When this method returns <see langword="true"/>, the <c>Any()</c>/<c>Count()</c> invocation or <c>Count</c>/<c>Length</c> member access; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="expression"/> is a recognized empty-check shape; otherwise, <see langword="false"/>.</returns>
    private static bool TryGetEmptyCheck(
        ExpressionSyntax expression,
        out ExpressionSyntax? target,
        out ExpressionSyntax? access
    )
    {
        var unwrapped = SyntaxHelpers.Unwrap(expression);
        target = null;
        access = null;

        if (unwrapped is PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.LogicalNotExpression } not)
        {
            if (
                SyntaxHelpers.Unwrap(not.Operand) is InvocationExpressionSyntax
                {
                    Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "Any" } anyAccess,
                    ArgumentList.Arguments.Count: 0,
                } anyInvocation
            )
            {
                target = anyAccess.Expression;
                access = anyInvocation;
                return true;
            }

            return false;
        }

        if (unwrapped is not BinaryExpressionSyntax { RawKind: (int)SyntaxKind.EqualsExpression } equals)
        {
            return false;
        }

        ExpressionSyntax countSide;

        if (SyntaxHelpers.IsZeroLiteral(equals.Right))
        {
            countSide = SyntaxHelpers.Unwrap(equals.Left);
        }
        else if (SyntaxHelpers.IsZeroLiteral(equals.Left))
        {
            countSide = SyntaxHelpers.Unwrap(equals.Right);
        }
        else
        {
            return false;
        }

        switch (countSide)
        {
            case MemberAccessExpressionSyntax { Name.Identifier.Text: "Count" or "Length" } memberAccess:
                target = memberAccess.Expression;
                access = memberAccess;
                return true;

            case InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "Count" } countAccess,
                ArgumentList.Arguments.Count: 0,
            } countInvocation:
                target = countAccess.Expression;
                access = countInvocation;
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Determines whether the recognized empty check is semantically equivalent to the one performed by the
    /// <c>ArgumentException.ThrowIfNullOrEmpty</c> overload the rewritten call would bind to, and whether that call
    /// would compile at all.
    /// </summary>
    /// <param name="emptyCheck">The <c>Any()</c>/<c>Count()</c> invocation or <c>Count</c>/<c>Length</c> member access.</param>
    /// <param name="argument">The collection argument being checked.</param>
    /// <param name="semanticModel">The semantic model used to resolve symbols and types.</param>
    /// <param name="cancellationToken">The token used to cancel semantic-model lookups.</param>
    /// <returns><see langword="true"/> if the throw-helper substitution preserves behavior and compiles; otherwise, <see langword="false"/>.</returns>
    private static bool IsSupportedEmptyCheck(
        ExpressionSyntax emptyCheck,
        ExpressionSyntax argument,
        SemanticModel semanticModel,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var type = semanticModel.GetTypeInfo(argument, cancellationToken).Type;

        if (type is null or IErrorTypeSymbol || !IsSupportedReceiverType(type, semanticModel.Compilation))
        {
            return false;
        }

        if (emptyCheck is InvocationExpressionSyntax invocation)
        {
            // "Any()"/"Count()" must be the LINQ extension methods, not an unrelated member of the same name.
            var symbol = semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol;
            var enumerableType = semanticModel.Compilation.GetTypeByMetadataName(EnumerableTypeMetadataName);

            return symbol is IMethodSymbol methodSymbol
                && enumerableType is not null
                && SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, enumerableType);
        }

        var memberAccess = (MemberAccessExpressionSyntax)emptyCheck;

        // "Length" is only accepted on arrays, so that e.g. string.Length never matches; "Count" must be an
        // instance property, not an unrelated field or method group.
        return memberAccess.Name.Identifier.Text == "Length"
            ? type is IArrayTypeSymbol
            : type is not IArrayTypeSymbol
                && semanticModel.GetSymbolInfo(memberAccess, cancellationToken).Symbol
                    is IPropertySymbol { IsStatic: false };
    }

    /// <summary>
    /// Determines whether <c>ArgumentException.ThrowIfNullOrEmpty(arg)</c> binds unambiguously to the
    /// <c>T[]</c>, <c>ICollection&lt;T&gt;</c>, or <c>IReadOnlyCollection&lt;T&gt;</c> overload for an argument of the given type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Types that implement both <c>ICollection&lt;T&gt;</c> and <c>IReadOnlyCollection&lt;T&gt;</c> (e.g. <c>List&lt;T&gt;</c>,
    /// <c>HashSet&lt;T&gt;</c>, <c>Dictionary&lt;TKey, TValue&gt;</c>) are rejected, since the call is ambiguous between those two
    /// overloads (CS0121). The same applies to types implementing either interface for more than one <c>T</c>, where type
    /// inference fails.
    /// </para>
    /// <para>
    /// Types that only implement <c>IEnumerable&lt;T&gt;</c> are rejected as well: that overload only throws when the
    /// element count can be determined without enumerating, so it is not equivalent to a manual <c>!arg.Any()</c> check on
    /// a lazily-evaluated sequence.
    /// </para>
    /// </remarks>
    /// <param name="type">The static type of the checked argument.</param>
    /// <param name="compilation">The compilation used to resolve the collection interfaces.</param>
    /// <returns><see langword="true"/> if the throw-helper call binds to a behavior-preserving overload; otherwise, <see langword="false"/>.</returns>
    private static bool IsSupportedReceiverType(ITypeSymbol type, Compilation compilation)
    {
        if (type is IArrayTypeSymbol { IsSZArray: true })
        {
            return true;
        }

        if (type is not INamedTypeSymbol)
        {
            // Type parameters, pointers, dynamic, multi-dimensional arrays, etc.
            return false;
        }

        var collectionInterface = compilation.GetTypeByMetadataName(CollectionInterfaceMetadataName);
        var readOnlyCollectionInterface = compilation.GetTypeByMetadataName(ReadOnlyCollectionInterfaceMetadataName);

        if (collectionInterface is null)
        {
            return false;
        }

        var candidates = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        foreach (var candidate in type.AllInterfaces.Append((INamedTypeSymbol)type))
        {
            var definition = candidate.OriginalDefinition;

            if (
                SymbolEqualityComparer.Default.Equals(definition, collectionInterface)
                || (
                    readOnlyCollectionInterface is not null
                    && SymbolEqualityComparer.Default.Equals(definition, readOnlyCollectionInterface)
                )
            )
            {
                _ = candidates.Add(candidate);
            }
        }

        return candidates.Count == 1;
    }
}
