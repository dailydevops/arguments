#if !NET9_0_OR_GREATER

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;

#pragma warning restore IDE0130 // Namespace does not match folder structure

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

/// <summary>Provides polyfill extension methods for <see cref="ArgumentNullException"/> to support newer .NET APIs in older framework versions.</summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "As designed.")]
public static class ArgumentNullExceptionPolyfills
{
    extension(ArgumentNullException)
    {
#if !NET6_0_OR_GREATER
        /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is <see langword="null"/>.</summary>
        /// <param name="argument">The reference type argument to validate as non-null.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds. If you omit this parameter, the name of <paramref name="argument"/> is used.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <remarks>The <paramref name="paramName"/> parameter is included to support the <see cref="CallerArgumentExpressionAttribute"/> attribute. It's recommended that you don't pass a value for this parameter and let the name of <paramref name="argument"/> be used instead.</remarks>
        /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception.throwifnull?view=net-10.0#system-argumentnullexception-throwifnull(system-object-system-string)" />
        public static void ThrowIfNull(
            [NotNull] object? argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
#endif

        /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is <see langword="null"/>.</summary>
        /// <param name="argument">The pointer argument to validate as non-null.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception.throwifnull?view=net-10.0#system-argumentnullexception-throwifnull(system-void*-system-string)" />
        public static unsafe void ThrowIfNull(
            void* argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}
#endif
