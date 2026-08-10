namespace NetEvolve.Arguments;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[SuppressMessage(
    "Maintainability",
    "NE0001:Declare one type per file with a matching file name",
    Justification = "Obsolete"
)]
public static partial class Argument
{
    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is not equal to <paramref name="other"/>.</summary>
    /// <param name="value">The argument to validate as equal to <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    [Obsolete("Use ArgumentOutOfRangeException.ThrowIfNotEqual instead.")]
    [DebuggerStepThrough]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNotEqual<T>(
        T value,
        T other,
        [CallerArgumentExpression(nameof(value))] string? paramName = null
    )
        where T : IEquatable<T> => ArgumentOutOfRangeException.ThrowIfNotEqual(value, other, paramName);
}
