namespace NetEvolve.Arguments;

using System.Runtime.CompilerServices;
using System;
using System.Diagnostics;

public static partial class Argument
{
    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is greater than <paramref name="other"/>.</summary>
    /// <param name="value">The argument to validate as less or equal than <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [StackTraceHidden]
    public static void ThrowIfGreaterThan<T>(
        T value,
        T other,
        [CallerArgumentExpression(nameof(value))] string? paramName = null
    )
        where T : IComparable<T>
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, other, paramName);
#else
        if (value.CompareTo(other) > 0)
        {
            throw new ArgumentOutOfRangeException(paramName, value, null);
        }
#endif
    }
}
