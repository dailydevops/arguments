namespace NetEvolve.Arguments;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public static partial class Argument
{
    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is equal to <paramref name="other"/>.</summary>
    /// <param name="value">The argument to validate as not equal to <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    [DebuggerStepThrough]
    [StackTraceHidden]
#if NET8_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
    [MethodImpl(MethodImplOptions.NoInlining)]
#endif
    public static void ThrowIfEqual<T>(
        T value,
        T other,
        [CallerArgumentExpression(nameof(value))] string? paramName = null
    )
        where T : IEquatable<T>
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfEqual(value, other, paramName);
#else
        if (EqualityComparer<T>.Default.Equals(value, other))
        {
            ThrowArgumentOutOfRangeException(
                paramName,
                value,
                $"{paramName} ('{value}') must not be equal to '{other}'."
            );
        }
#endif
    }
}
