namespace NetEvolve.Arguments;

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public static partial class Argument
{
    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is greater than or equal <paramref name="other"/>.</summary>
    /// <param name="value">The argument to validate as less than <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    [Obsolete("Use ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual instead.")]
    [DebuggerStepThrough]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfGreaterThanOrEqual<T>(
        T value,
        T other,
        [CallerArgumentExpression(nameof(value))] string? paramName = null
    )
        where T : IComparable<T> => ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(value, other, paramName);
}
