#if !NET8_0_OR_GREATER
namespace NetEvolve.Arguments;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

public static partial class Argument
{
    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/>.</summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    [StackTraceHidden]
    private static void ThrowArgumentOutOfRangeException<T>(string? paramName, T value)
        where T : IComparable<T>
    {
        throw new ArgumentOutOfRangeException(paramName, value, null);
    }
}
#endif
