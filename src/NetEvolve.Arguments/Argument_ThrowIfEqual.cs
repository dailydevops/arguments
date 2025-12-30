namespace NetEvolve.Arguments;

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public static partial class Argument
{
    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is equal to <paramref name="other"/>.</summary>
    /// <param name="value">The argument to validate as not equal to <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    [Obsolete("Use ArgumentOutOfRangeException.ThrowIfEqual instead.")]
    [DebuggerStepThrough]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfEqual<T>(
        T value,
        T other,
        [CallerArgumentExpression(nameof(value))] string? paramName = null
    )
        where T : IEquatable<T> => ArgumentOutOfRangeException.ThrowIfEqual(value, other, paramName);
}
