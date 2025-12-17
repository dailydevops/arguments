namespace System;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using NetEvolve.Arguments;

[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "As designed.")]
public static class ArgumentNullExceptionExtensions
{
    extension(ArgumentNullException)
    {
        public static void ThrowIfNull(
            [NotNull] object? argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                Argument.ThrowArgumentNullException(paramName);
            }
        }

        public static unsafe void ThrowIfNull(
            [NotNull] void* argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument == null)
            {
                Argument.ThrowArgumentNullException(paramName);
            }
        }
    }
}
