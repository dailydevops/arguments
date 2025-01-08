namespace NetEvolve.Arguments;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

public static partial class Argument
{
    /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
    /// <param name="argument">The reference type argument to validate as non-null.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    [DebuggerStepThrough]
    [StackTraceHidden]
#if NET6_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
    [MethodImpl(MethodImplOptions.NoInlining)]
#endif
    public static void ThrowIfNull(
        [NotNull] object? argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null
    )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(argument, paramName);
#else
        if (argument is null)
        {
            ThrowArgumentNullException(paramName);
        }
#endif
    }

    /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
    /// <param name="argument">The reference type argument to validate as non-null.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    [DebuggerStepThrough]
    [StackTraceHidden]
#if NET7_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
    [MethodImpl(MethodImplOptions.NoInlining)]
#endif
#pragma warning disable S6640 // Make sure that using "unsafe" is safe here.
    public static unsafe void ThrowIfNull(
#pragma warning restore S6640 // Make sure that using "unsafe" is safe here.
        [NotNull] void* argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null
    )
    {
#if NET7_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(argument, paramName);
#else
        if (argument == null)
        {
            ThrowArgumentNullException(paramName);
        }
#endif
    }
}
