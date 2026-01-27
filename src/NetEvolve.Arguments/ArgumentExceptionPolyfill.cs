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

        /// <summary>Throws an exception if <paramref name="argument"/> is <see langword="null"/> or empty.</summary>
        /// <typeparam name="T">The type of objects in the collection.</typeparam>
        /// <param name="argument">The collection argument to validate as non-<see langword="null"/> and non-empty.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="argument"/> is empty.</exception>
        public static void ThrowIfNullOrEmpty<T>(
            [NotNull] ICollection<T>? argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (argument.Count == 0)
            {
                throw new ArgumentException("The collection cannot be empty.", paramName);
            }
        }

        /// <summary>Throws an exception if <paramref name="argument"/> is <see langword="null"/> or empty.</summary>
        /// <typeparam name="T">The type of objects in the collection.</typeparam>
        /// <param name="argument">The collection argument to validate as non-<see langword="null"/> and non-empty.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="argument"/> is empty.</exception>
        public static void ThrowIfNullOrEmpty<T>(
            [NotNull] IReadOnlyCollection<T>? argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (argument.Count == 0)
            {
                throw new ArgumentException("The collection cannot be empty.", paramName);
            }
        }

        /// <summary>Throws an exception if <paramref name="argument"/> is <see langword="null"/> or empty.</summary>
        /// <typeparam name="T">The type of objects in the array.</typeparam>
        /// <param name="argument">The array argument to validate as non-<see langword="null"/> and non-empty.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="argument"/> is empty.</exception>
        public static void ThrowIfNullOrEmpty<T>(
            [NotNull] T[]? argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (argument.Length == 0)
            {
                throw new ArgumentException("The array cannot be empty.", paramName);
            }
        }

        /// <summary>Throws an exception if <paramref name="argument"/> exceeds the specified maximum length.</summary>
        /// <param name="argument">The string argument to validate.</param>
        /// <param name="maxLength">The maximum allowed length.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="argument"/> length exceeds <paramref name="maxLength"/>.</exception>
        public static void ThrowIfLengthGreaterThan(
            [NotNull] string? argument,
            int maxLength,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (argument.Length > maxLength)
            {
                throw new ArgumentException(
                    $"The string length {argument.Length} exceeds the maximum allowed length {maxLength}.",
                    paramName
                );
            }
        }

        /// <summary>Throws an exception if <paramref name="argument"/> is less than the specified minimum length.</summary>
        /// <param name="argument">The string argument to validate.</param>
        /// <param name="minLength">The minimum required length.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="argument"/> length is less than <paramref name="minLength"/>.</exception>
        public static void ThrowIfLengthLessThan(
            [NotNull] string? argument,
            int minLength,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (argument.Length < minLength)
            {
                throw new ArgumentException(
                    $"The string length {argument.Length} is less than the minimum required length {minLength}.",
                    paramName
                );
            }
        }

        /// <summary>Throws an exception if <paramref name="argument"/> length is outside the specified range [<paramref name="minLength"/>, <paramref name="maxLength"/>].</summary>
        /// <param name="argument">The string argument to validate.</param>
        /// <param name="minLength">The minimum required length (inclusive).</param>
        /// <param name="maxLength">The maximum allowed length (inclusive).</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="argument"/> length is outside the specified range.</exception>
        public static void ThrowIfLengthOutOfRange(
            [NotNull] string? argument,
            int minLength,
            int maxLength,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (argument.Length < minLength || argument.Length > maxLength)
            {
                throw new ArgumentException(
                    $"The string length {argument.Length} is outside the allowed range [{minLength}, {maxLength}].",
                    paramName
                );
            }
        }

        /// <summary>Throws an exception if <paramref name="argument"/> contains any white-space characters.</summary>
        /// <param name="argument">The string argument to validate.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="argument"/> contains white-space characters.</exception>
        public static void ThrowIfContainsWhiteSpace(
            [NotNull] string? argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (argument.Any(c => char.IsWhiteSpace(c)))
            {
                throw new ArgumentException("The string cannot contain white-space characters.", paramName);
            }
        }

        /// <summary>Throws an exception if the count of <paramref name="argument"/> exceeds the specified maximum.</summary>
        /// <typeparam name="T">The type of objects to enumerate.</typeparam>
        /// <param name="argument">The enumerable argument to validate.</param>
        /// <param name="maxCount">The maximum allowed count.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The count of <paramref name="argument"/> exceeds <paramref name="maxCount"/>.</exception>
        public static void ThrowIfCountGreaterThan<T>(
            [NotNull] IEnumerable<T>? argument,
            int maxCount,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (argument.TryGetNonEnumeratedCount(out var count) && count > maxCount)
            {
                throw new ArgumentException(
                    $"The collection count {count} exceeds the maximum allowed count {maxCount}.",
                    paramName
                );
            }
        }

        /// <summary>Throws an exception if the count of <paramref name="argument"/> exceeds the specified maximum.</summary>
        /// <typeparam name="T">The type of objects in the collection.</typeparam>
        /// <param name="argument">The collection argument to validate.</param>
        /// <param name="maxCount">The maximum allowed count.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The count of <paramref name="argument"/> exceeds <paramref name="maxCount"/>.</exception>
        public static void ThrowIfCountGreaterThan<T>(
            [NotNull] ICollection<T>? argument,
            int maxCount,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (argument.Count > maxCount)
            {
                throw new ArgumentException(
                    $"The collection count {argument.Count} exceeds the maximum allowed count {maxCount}.",
                    paramName
                );
            }
        }

        /// <summary>Throws an exception if the count of <paramref name="argument"/> exceeds the specified maximum.</summary>
        /// <typeparam name="T">The type of objects in the collection.</typeparam>
        /// <param name="argument">The collection argument to validate.</param>
        /// <param name="maxCount">The maximum allowed count.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The count of <paramref name="argument"/> exceeds <paramref name="maxCount"/>.</exception>
        public static void ThrowIfCountGreaterThan<T>(
            [NotNull] IReadOnlyCollection<T>? argument,
            int maxCount,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (argument.Count > maxCount)
            {
                throw new ArgumentException(
                    $"The collection count {argument.Count} exceeds the maximum allowed count {maxCount}.",
                    paramName
                );
            }
        }

        /// <summary>Throws an exception if the length of <paramref name="argument"/> exceeds the specified maximum.</summary>
        /// <typeparam name="T">The type of objects in the array.</typeparam>
        /// <param name="argument">The array argument to validate.</param>
        /// <param name="maxCount">The maximum allowed length.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The length of <paramref name="argument"/> exceeds <paramref name="maxCount"/>.</exception>
        public static void ThrowIfCountGreaterThan<T>(
            [NotNull] T[]? argument,
            int maxCount,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (argument.Length > maxCount)
            {
                throw new ArgumentException(
                    $"The array length {argument.Length} exceeds the maximum allowed count {maxCount}.",
                    paramName
                );
            }
        }

        /// <summary>Throws an exception if the count of <paramref name="argument"/> is less than the specified minimum.</summary>
        /// <typeparam name="T">The type of objects to enumerate.</typeparam>
        /// <param name="argument">The enumerable argument to validate.</param>
        /// <param name="minCount">The minimum required count.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The count of <paramref name="argument"/> is less than <paramref name="minCount"/>.</exception>
        public static void ThrowIfCountLessThan<T>(
            [NotNull] IEnumerable<T>? argument,
            int minCount,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (argument.TryGetNonEnumeratedCount(out var count) && count < minCount)
            {
                throw new ArgumentException(
                    $"The collection count {count} is less than the minimum required count {minCount}.",
                    paramName
                );
            }
        }

        /// <summary>Throws an exception if the count of <paramref name="argument"/> is less than the specified minimum.</summary>
        /// <typeparam name="T">The type of objects in the collection.</typeparam>
        /// <param name="argument">The collection argument to validate.</param>
        /// <param name="minCount">The minimum required count.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The count of <paramref name="argument"/> is less than <paramref name="minCount"/>.</exception>
        public static void ThrowIfCountLessThan<T>(
            [NotNull] ICollection<T>? argument,
            int minCount,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (argument.Count < minCount)
            {
                throw new ArgumentException(
                    $"The collection count {argument.Count} is less than the minimum required count {minCount}.",
                    paramName
                );
            }
        }

        /// <summary>Throws an exception if the count of <paramref name="argument"/> is less than the specified minimum.</summary>
        /// <typeparam name="T">The type of objects in the collection.</typeparam>
        /// <param name="argument">The collection argument to validate.</param>
        /// <param name="minCount">The minimum required count.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The count of <paramref name="argument"/> is less than <paramref name="minCount"/>.</exception>
        public static void ThrowIfCountLessThan<T>(
            [NotNull] IReadOnlyCollection<T>? argument,
            int minCount,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (argument.Count < minCount)
            {
                throw new ArgumentException(
                    $"The collection count {argument.Count} is less than the minimum required count {minCount}.",
                    paramName
                );
            }
        }

        /// <summary>Throws an exception if the length of <paramref name="argument"/> is less than the specified minimum.</summary>
        /// <typeparam name="T">The type of objects in the array.</typeparam>
        /// <param name="argument">The array argument to validate.</param>
        /// <param name="minCount">The minimum required length.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The length of <paramref name="argument"/> is less than <paramref name="minCount"/>.</exception>
        public static void ThrowIfCountLessThan<T>(
            [NotNull] T[]? argument,
            int minCount,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (argument.Length < minCount)
            {
                throw new ArgumentException(
                    $"The array length {argument.Length} is less than the minimum required count {minCount}.",
                    paramName
                );
            }
        }

        /// <summary>Throws an exception if the count of <paramref name="argument"/> is outside the specified range [<paramref name="minCount"/>, <paramref name="maxCount"/>].</summary>
        /// <typeparam name="T">The type of objects to enumerate.</typeparam>
        /// <param name="argument">The enumerable argument to validate.</param>
        /// <param name="minCount">The minimum required count (inclusive).</param>
        /// <param name="maxCount">The maximum allowed count (inclusive).</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The count of <paramref name="argument"/> is outside the specified range.</exception>
        public static void ThrowIfCountOutOfRange<T>(
            [NotNull] IEnumerable<T>? argument,
            int minCount,
            int maxCount,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (argument.TryGetNonEnumeratedCount(out var count) && (count < minCount || count > maxCount))
            {
                throw new ArgumentException(
                    $"The collection count {count} is outside the allowed range [{minCount}, {maxCount}].",
                    paramName
                );
            }
        }

        /// <summary>Throws an exception if the count of <paramref name="argument"/> is outside the specified range [<paramref name="minCount"/>, <paramref name="maxCount"/>].</summary>
        /// <typeparam name="T">The type of objects in the collection.</typeparam>
        /// <param name="argument">The collection argument to validate.</param>
        /// <param name="minCount">The minimum required count (inclusive).</param>
        /// <param name="maxCount">The maximum allowed count (inclusive).</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The count of <paramref name="argument"/> is outside the specified range.</exception>
        public static void ThrowIfCountOutOfRange<T>(
            [NotNull] ICollection<T>? argument,
            int minCount,
            int maxCount,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (argument.Count < minCount || argument.Count > maxCount)
            {
                throw new ArgumentException(
                    $"The collection count {argument.Count} is outside the allowed range [{minCount}, {maxCount}].",
                    paramName
                );
            }
        }

        /// <summary>Throws an exception if the count of <paramref name="argument"/> is outside the specified range [<paramref name="minCount"/>, <paramref name="maxCount"/>].</summary>
        /// <typeparam name="T">The type of objects in the collection.</typeparam>
        /// <param name="argument">The collection argument to validate.</param>
        /// <param name="minCount">The minimum required count (inclusive).</param>
        /// <param name="maxCount">The maximum allowed count (inclusive).</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The count of <paramref name="argument"/> is outside the specified range.</exception>
        public static void ThrowIfCountOutOfRange<T>(
            [NotNull] IReadOnlyCollection<T>? argument,
            int minCount,
            int maxCount,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (argument.Count < minCount || argument.Count > maxCount)
            {
                throw new ArgumentException(
                    $"The collection count {argument.Count} is outside the allowed range [{minCount}, {maxCount}].",
                    paramName
                );
            }
        }

        /// <summary>Throws an exception if the length of <paramref name="argument"/> is outside the specified range [<paramref name="minCount"/>, <paramref name="maxCount"/>].</summary>
        /// <typeparam name="T">The type of objects in the array.</typeparam>
        /// <param name="argument">The array argument to validate.</param>
        /// <param name="minCount">The minimum required length (inclusive).</param>
        /// <param name="maxCount">The maximum allowed length (inclusive).</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The length of <paramref name="argument"/> is outside the specified range.</exception>
        public static void ThrowIfCountOutOfRange<T>(
            [NotNull] T[]? argument,
            int minCount,
            int maxCount,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (argument.Length < minCount || argument.Length > maxCount)
            {
                throw new ArgumentException(
                    $"The array length {argument.Length} is outside the allowed range [{minCount}, {maxCount}].",
                    paramName
                );
            }
        }

        /// <summary>Throws an exception if <paramref name="argument"/> contains duplicate elements.</summary>
        /// <typeparam name="T">The type of objects to enumerate.</typeparam>
        /// <param name="argument">The enumerable argument to validate.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="argument"/> contains duplicate elements.</exception>
        public static void ThrowIfContainsDuplicates<T>(
            [NotNull] IEnumerable<T>? argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            var set = new HashSet<T>();
            if (!argument.All(item => set.Add(item)))
            {
                throw new ArgumentException("The collection cannot contain duplicate elements.", paramName);
            }
        }

        /// <summary>Throws an exception if <paramref name="argument"/> contains duplicate elements using the specified equality comparer.</summary>
        /// <typeparam name="T">The type of objects to enumerate.</typeparam>
        /// <param name="argument">The enumerable argument to validate.</param>
        /// <param name="comparer">The equality comparer to use for comparing elements.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="argument"/> contains duplicate elements.</exception>
        public static void ThrowIfContainsDuplicates<T>(
            [NotNull] IEnumerable<T>? argument,
            IEqualityComparer<T>? comparer,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            var set = new HashSet<T>(comparer);

            if (!argument.All(item => set.Add(item)))
            {
                throw new ArgumentException("The collection cannot contain duplicate elements.", paramName);
            }
        }

        /// <summary>Throws an exception if <paramref name="argument"/> is equal to its default value.</summary>
        /// <typeparam name="T">The type of the value to validate.</typeparam>
        /// <param name="argument">The argument to validate as non-default.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentException"><paramref name="argument"/> is equal to its default value.</exception>
        public static void ThrowIfDefault<T>(
            T argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
            where T : struct, IEquatable<T>
        {
            if (argument.Equals(default))
            {
                throw new ArgumentException("The value cannot be the default value.", paramName);
            }
        }

        /// <summary>Throws an exception if <paramref name="argument"/> is equal to <see cref="Guid.Empty"/>.</summary>
        /// <param name="argument">The GUID argument to validate as non-empty.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentException"><paramref name="argument"/> is equal to <see cref="Guid.Empty"/>.</exception>
        public static void ThrowIfEmptyGuid(
            Guid argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null
        )
        {
            if (argument == Guid.Empty)
            {
                throw new ArgumentException("The GUID cannot be empty.", paramName);
            }
        }
    }
}
