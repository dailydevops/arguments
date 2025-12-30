#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;

#pragma warning restore IDE0130 // Namespace does not match folder structure

using System.Diagnostics.CodeAnalysis;

/// <summary>Provides polyfill extension methods for <see cref="ArgumentOutOfRangeException"/> to support newer .NET APIs in older framework versions.</summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "As designed.")]
public static class ArgumentOutOfRangeExceptionPolyfills
{
    extension(ArgumentOutOfRangeException)
    {
#if !NET8_0_OR_GREATER
        /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is zero.</summary>
        /// <typeparam name="T">The type of the object to validate.</typeparam>
        /// <param name="value">The argument to validate as non-zero.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is zero.</exception>
        /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifzero?view=net-10.0#system-argumentoutofrangeexception-throwifzero-1(-0-system-string)" />
        public static void ThrowIfZero<T>(
            T value,
            [Runtime.CompilerServices.CallerArgumentExpression(nameof(value))] string? paramName = null
        )
#if NET7_0_OR_GREATER
            where T : Numerics.INumberBase<T>
        {
            if (T.IsZero(value))
            {
                ThrowZero(paramName);
            }
        }
#else
            where T : struct, IEquatable<T>
        {
            if (value.Equals(default))
            {
                ThrowZero(paramName);
            }
        }
#endif

        [DoesNotReturn]
        private static void ThrowZero(string? paramName) =>
            throw new ArgumentOutOfRangeException(paramName, "Value must not be zero.");
#endif

#if !NET8_0_OR_GREATER
        /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative.</summary>
        /// <typeparam name="T">The type of the object to validate.</typeparam>
        /// <param name="value">The argument to validate as non-negative.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.</exception>
        /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifnegative?view=net-10.0#system-argumentoutofrangeexception-throwifnegative-1(-0-system-string)" />
        public static void ThrowIfNegative<T>(
            T value,
            [Runtime.CompilerServices.CallerArgumentExpression(nameof(value))] string? paramName = null
        )
#if NET7_0_OR_GREATER
            where T : Numerics.INumberBase<T>
        {
            if (T.IsNegative(value))
            {
                ThrowNegative(value, paramName);
            }
        }
#else
            where T : struct, IComparable<T>
        {
            if (value.CompareTo(default) < 0)
            {
                ThrowNegative(value, paramName);
            }
        }
#endif

#if !NET7_0_OR_GREATER
        /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative.</summary>
        /// <param name="value">The argument to validate as non-negative.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.</exception>
        /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifnegative?view=net-10.0#system-argumentoutofrangeexception-throwifnegative-1(-0-system-string)" />
        public static void ThrowIfNegative(
            nint value,
            [Runtime.CompilerServices.CallerArgumentExpression(nameof(value))] string? paramName = null
        )
        {
            if (value < (nint)0)
            {
                ThrowNegative(value, paramName);
            }
        }
#endif

        [DoesNotReturn]
        private static void ThrowNegative<T>(T value, string? paramName) =>
            throw new ArgumentOutOfRangeException(paramName, value, "Value must be non-negative.");

#endif

#if !NET8_0_OR_GREATER
        /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative or zero.</summary>
        /// <typeparam name="T">The type of the object to validate.</typeparam>
        /// <param name="value">The argument to validate as non-zero and non-negative.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative or zero.</exception>
        /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifnegativeorzero?view=net-10.0#system-argumentoutofrangeexception-throwifnegativeorzero-1(-0-system-string)" />
        public static void ThrowIfNegativeOrZero<T>(
            T value,
            [Runtime.CompilerServices.CallerArgumentExpression(nameof(value))] string? paramName = null
        )
#if NET7_0_OR_GREATER
            where T : Numerics.INumberBase<T>
        {
            if (T.IsNegative(value) || T.IsZero(value))
            {
                ThrowNegativeOrZero(value, paramName);
            }
        }
#else
            where T : struct, IComparable<T>
        {
            if (value.CompareTo(default) <= 0)
            {
                ThrowNegativeOrZero(value, paramName);
            }
        }
#endif
#endif

#if !NET7_0_OR_GREATER
        /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative or zero.</summary>
        /// <param name="value">The argument to validate as non-zero and non-negative.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative or zero.</exception>
        /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifnegativeorzero?view=net-10.0#system-argumentoutofrangeexception-throwifnegativeorzero-1(-0-system-string)" />
        public static void ThrowIfNegativeOrZero(
            nint value,
            [Runtime.CompilerServices.CallerArgumentExpression(nameof(value))] string? paramName = null
        )
        {
            if (value <= (nint)0)
            {
                ThrowNegativeOrZero(value, paramName);
            }
        }
#endif

        [DoesNotReturn]
        private static void ThrowNegativeOrZero<T>(T value, string? name) =>
            throw new ArgumentOutOfRangeException(
                name,
                value,
                $"{name} ('{value}') must be a non-negative and non-zero value."
            );

#if !NET8_0_OR_GREATER
        /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is equal to <paramref name="other"/>.</summary>
        /// <typeparam name="T">The type of the objects to validate.</typeparam>
        /// <param name="value">The argument to validate as not equal to <paramref name="other"/>.</param>
        /// <param name="other">The value to compare with <paramref name="value"/>.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is equal to <paramref name="other"/>.</exception>
        /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifequal?view=net-10.0#system-argumentoutofrangeexception-throwifequal-1(-0-0-system-string)" />
        public static void ThrowIfEqual<T>(
            T value,
            T other,
            [Runtime.CompilerServices.CallerArgumentExpression(nameof(value))] string? paramName = null
        )
            where T : IEquatable<T>?
        {
            if (EqualityComparer<T>.Default.Equals(value, other))
            {
                throw new ArgumentOutOfRangeException(paramName, value, $"Value must not be equal to {other}.");
            }
        }
#endif

#if !NET8_0_OR_GREATER
        /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is not equal to <paramref name="other"/>.</summary>
        /// <typeparam name="T">The type of the objects to validate.</typeparam>
        /// <param name="value">The argument to validate as equal to <paramref name="other"/>.</param>
        /// <param name="other">The value to compare with <paramref name="value"/>.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not equal to <paramref name="other"/>.</exception>
        /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifnotequal?view=net-10.0#system-argumentoutofrangeexception-throwifnotequal-1(-0-0-system-string)" />
        public static void ThrowIfNotEqual<T>(
            T value,
            T other,
            [Runtime.CompilerServices.CallerArgumentExpression(nameof(value))] string? paramName = null
        )
            where T : IEquatable<T>?
        {
            if (!EqualityComparer<T>.Default.Equals(value, other))
            {
                throw new ArgumentOutOfRangeException(paramName, value, $"Value must be equal to {other}.");
            }
        }
#endif

#if !NET8_0_OR_GREATER
        /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is greater than <paramref name="other"/>.</summary>
        /// <typeparam name="T">The type of the objects to compare.</typeparam>
        /// <param name="value">The argument to validate as less than or equal to <paramref name="other"/>.</param>
        /// <param name="other">The value to compare with <paramref name="value"/>.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is greater than <paramref name="other"/>.</exception>
        /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifgreaterthan?view=net-10.0#system-argumentoutofrangeexception-throwifgreaterthan-1(-0-0-system-string)" />
        public static void ThrowIfGreaterThan<T>(
            T value,
            T other,
            [Runtime.CompilerServices.CallerArgumentExpression(nameof(value))] string? paramName = null
        )
            where T : IComparable<T>
        {
            if (value.CompareTo(other) > 0)
            {
                throw new ArgumentOutOfRangeException(
                    paramName,
                    value,
                    $"Value must be less than or equal to {other}."
                );
            }
        }
#endif

#if !NET8_0_OR_GREATER
        /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is greater than or equal to <paramref name="other"/>.</summary>
        /// <typeparam name="T">The type of the objects to compare.</typeparam>
        /// <param name="value">The argument to validate as less than <paramref name="other"/>.</param>
        /// <param name="other">The value to compare with <paramref name="value"/>.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is greater than or equal to <paramref name="other"/>.</exception>
        /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifgreaterthanorequal?view=net-10.0#system-argumentoutofrangeexception-throwifgreaterthanorequal-1(-0-0-system-string)" />
        public static void ThrowIfGreaterThanOrEqual<T>(
            T value,
            T other,
            [Runtime.CompilerServices.CallerArgumentExpression(nameof(value))] string? paramName = null
        )
            where T : IComparable<T>
        {
            if (value.CompareTo(other) >= 0)
            {
                throw new ArgumentOutOfRangeException(paramName, value, $"Value must be less than {other}.");
            }
        }
#endif

#if !NET8_0_OR_GREATER
        /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than <paramref name="other"/>.</summary>
        /// <typeparam name="T">The type of the objects to compare.</typeparam>
        /// <param name="value">The argument to validate as greater than or equal to <paramref name="other"/>.</param>
        /// <param name="other">The value to compare with <paramref name="value"/>.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than <paramref name="other"/>.</exception>
        /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwiflessthan?view=net-10.0#system-argumentoutofrangeexception-throwiflessthan-1(-0-0-system-string)" />
        public static void ThrowIfLessThan<T>(
            T value,
            T other,
            [Runtime.CompilerServices.CallerArgumentExpression(nameof(value))] string? paramName = null
        )
            where T : IComparable<T>
        {
            if (value.CompareTo(other) < 0)
            {
                throw new ArgumentOutOfRangeException(
                    paramName,
                    value,
                    $"Value must be greater than or equal to {other}."
                );
            }
        }
#endif

#if !NET8_0_OR_GREATER
        /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than or equal to <paramref name="other"/>.</summary>
        /// <typeparam name="T">The type of the objects to compare.</typeparam>
        /// <param name="value">The argument to validate as greater than <paramref name="other"/>.</param>
        /// <param name="other">The value to compare with <paramref name="value"/>.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than or equal to <paramref name="other"/>.</exception>
        /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwiflessthanorequal?view=net-10.0#system-argumentoutofrangeexception-throwiflessthanorequal-1(-0-0-system-string)" />
        public static void ThrowIfLessThanOrEqual<T>(
            T value,
            T other,
            [Runtime.CompilerServices.CallerArgumentExpression(nameof(value))] string? paramName = null
        )
            where T : IComparable<T>
        {
            if (value.CompareTo(other) <= 0)
            {
                throw new ArgumentOutOfRangeException(paramName, value, $"Value must be greater than {other}.");
            }
        }
#endif
    }
}
