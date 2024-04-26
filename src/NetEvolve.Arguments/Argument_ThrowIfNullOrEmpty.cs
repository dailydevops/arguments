namespace NetEvolve.Arguments;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

public static partial class Argument
{
    /// <summary>Throws an exception if <paramref name="argument"/> is null or empty.</summary>
    /// <param name="argument">The string argument to validate as non-null and non-empty.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="argument"/> is empty.</exception>
    [DebuggerStepThrough]
    [StackTraceHidden]
#if NET7_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
    [MethodImpl(MethodImplOptions.NoInlining)]
#endif
    public static void ThrowIfNullOrEmpty(
        [NotNull] string? argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null
    )
    {
#if NET7_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(argument, paramName);
#else
        if (argument is null)
        {
            ThrowArgumentNullException(paramName);
        }

        if (string.IsNullOrEmpty(argument))
        {
            ThrowArgumentException(paramName);
        }
#endif
    }

    /// <summary>Throws an exception if <paramref name="argument"/> is null or empty.</summary>
    /// <param name="argument">The string argument to validate as non-null and non-empty.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="argument"/> is empty.</exception>
    [DebuggerStepThrough]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowIfNullOrEmpty<T>(
        [NotNull] IEnumerable<T> argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null
    )
    {
        if (argument is null)
        {
            ThrowArgumentNullException(paramName);
        }

        if (argument.TryGetNonEnumeratedCount(out var count) && count == 0)
        {
            ThrowArgumentException(paramName);
        }
    }
}
