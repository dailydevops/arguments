#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;

#pragma warning restore IDE0130 // Namespace does not match folder structure

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

/// <summary>Provides polyfill extension methods for <see cref="ArgumentException"/> to support newer .NET APIs in older framework versions.</summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "As designed.")]
public static class ArgumentExceptionPolyfills
{
    extension(ArgumentException)
    {
#if !NET8_0_OR_GREATER
        /// <summary>Throws an exception if <paramref name="argument"/> is <see langword="null"/> or empty.</summary>
        /// <param name="argument">The string argument to validate as non-<see langword="null"/> and non-empty.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="argument"/> is empty.</exception>
        /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception.throwifnullorempty?view=net-10.0#system-argumentexception-throwifnullorempty(system-string-system-string)" />
        public static void ThrowIfNullOrEmpty(
            [NotNull] string? argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (argument.Length == 0)
            {
                throw new ArgumentException("The value cannot be an empty string.", paramName);
            }
        }

        /// <summary>Throws an exception if <paramref name="argument"/> is <see langword="null"/>, empty, or consists only of white-space characters.</summary>
        /// <param name="argument">The string argument to validate.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="argument"/> is empty or consists only of white-space characters.</exception>
        /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception.throwifnullorwhitespace?view=net-10.0#system-argumentexception-throwifnullorwhitespace(system-string-system-string)" />
        public static void ThrowIfNullOrWhiteSpace(
            [NotNull] string? argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new ArgumentException(
                    "The value cannot be an empty string or composed entirely of whitespace.",
                    paramName
                );
            }
        }
#endif

        /// <summary>Throws an exception if <paramref name="argument"/> is <see langword="null"/> or empty.</summary>
        /// <typeparam name="T">The type of objects to enumerate.</typeparam>
        /// <param name="argument">The enumerable argument to validate as non-<see langword="null"/> and non-empty.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="argument"/> is empty.</exception>
        public static void ThrowIfNullOrEmpty<T>(
            [NotNull] IEnumerable<T>? argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (argument.TryGetNonEnumeratedCount(out var count) && count == 0)
            {
                throw new ArgumentException("The collection cannot be empty.", paramName);
            }
        }
    }
}
