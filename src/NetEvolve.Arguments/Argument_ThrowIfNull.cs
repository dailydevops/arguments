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
    [Obsolete("Use ArgumentNullException.ThrowIfNull instead.")]
    [DebuggerStepThrough]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNull(
        [NotNull] object? argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null
    ) => ArgumentNullException.ThrowIfNull(argument, paramName);

    /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
    /// <param name="argument">The reference type argument to validate as non-null.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    [Obsolete("Use ArgumentNullException.ThrowIfNull instead.")]
    [DebuggerStepThrough]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable S6640 // Make sure that using "unsafe" is safe here.
    public static unsafe void ThrowIfNull(
#pragma warning restore S6640 // Make sure that using "unsafe" is safe here.
        [NotNull] void* argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null
    ) => ArgumentNullException.ThrowIfNull(argument, paramName);
}
