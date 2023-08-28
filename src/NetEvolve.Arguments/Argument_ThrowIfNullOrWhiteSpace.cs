namespace NetEvolve.Arguments;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

public static partial class Argument
{
    /// <summary>Throws an exception if <paramref name="argument"/> is null, empty, or consists only of white-space characters.</summary>
    /// <param name="argument">The string argument to validate.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="argument"/> is empty or consists only of white-space characters.</exception>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [StackTraceHidden]
    public static void ThrowIfNullOrWhiteSpace(
        [NotNull] string? argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null
    )
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(argument, paramName);
#else
        if (argument is null)
        {
            throw new ArgumentNullException(paramName);
        }

        if (string.IsNullOrWhiteSpace(argument))
        {
            throw new ArgumentException(null, paramName);
        }
#endif
    }
}
