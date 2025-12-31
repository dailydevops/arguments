#if !NET6_0_OR_GREATER

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Linq;

#pragma warning restore IDE0130 // Namespace does not match folder structure

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
internal static class IEnumerableExtensions
{
    /// <summary>Attempts to determine the number of elements in a sequence without forcing an enumeration.</summary>
    /// <typeparam name="TSource">The type of elements in the source sequence.</typeparam>
    /// <param name="target">The source sequence to get the count from.</param>
    /// <param name="count">The count of elements in the sequence, if available; otherwise, <c>0</c>.</param>
    /// <returns>
    ///   <see langword="true" /> if the number of elements could be determined without enumeration;
    ///   otherwise, <see langword="false" />.
    /// </returns>
    /// <remarks>
    ///   <para>
    ///     This method attempts to determine the number of elements in a sequence without forcing
    ///     an enumeration by checking for common collection implementations such as
    ///     <see cref="ICollection{T}" /> and <see cref="ICollection" />.
    ///   </para>
    ///   <para>
    ///     If the sequence does not implement one of these interfaces or the count is not immediately
    ///     available, the method returns <see langword="false" /> and sets <paramref name="count" />
    ///     to <c>0</c>.
    ///   </para>
    /// </remarks>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.trygetnonenumeratedcount?view=net-10.0" />
    public static bool TryGetNonEnumeratedCount<TSource>(this IEnumerable<TSource> target, out int count)
    {
        if (target is ICollection<TSource> genericCollection)
        {
            count = genericCollection.Count;
            return true;
        }

        if (target is ICollection collection)
        {
            count = collection.Count;
            return true;
        }

        count = 0;
        return false;
    }
}
#endif
