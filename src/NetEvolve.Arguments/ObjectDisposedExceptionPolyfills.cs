#if !NET7_0_OR_GREATER

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;

#pragma warning restore IDE0130 // Namespace does not match folder structure

using System.Diagnostics.CodeAnalysis;

/// <summary>Provides polyfill extension methods for <see cref="ObjectDisposedException"/> to support newer .NET APIs in older framework versions.</summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "As designed.")]
public static class ObjectDisposedExceptionPolyfills
{
    extension(ObjectDisposedException)
    {
        /// <summary>Throws an <see cref="ObjectDisposedException"/> if the specified <paramref name="condition"/> is <see langword="true"/>.</summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="instance">The object whose type's full name should be included in any resulting <see cref="ObjectDisposedException"/>.</param>
        /// <exception cref="ObjectDisposedException">The <paramref name="condition"/> is <see langword="true"/>.</exception>
        /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.objectdisposedexception.throwif?view=net-10.0#system-objectdisposedexception-throwif(system-boolean-system-object)" />
        public static void ThrowIf([DoesNotReturnIf(true)] bool condition, object instance)
        {
            if (condition)
            {
                ThrowObjectDisposedException(instance);
            }
        }

        /// <summary>Throws an <see cref="ObjectDisposedException"/> if the specified <paramref name="condition"/> is <see langword="true"/>.</summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="type">The type whose full name should be included in any resulting <see cref="ObjectDisposedException"/>.</param>
        /// <exception cref="ObjectDisposedException">The <paramref name="condition"/> is <see langword="true"/>.</exception>
        /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.objectdisposedexception.throwif?view=net-10.0#system-objectdisposedexception-throwif(system-boolean-system-type)" />
        public static void ThrowIf([DoesNotReturnIf(true)] bool condition, Type type)
        {
            if (condition)
            {
                ThrowObjectDisposedException(type);
            }
        }

        /// <summary>Throws an <see cref="ObjectDisposedException"/> with the object name derived from the specified <paramref name="instance"/>.</summary>
        /// <param name="instance">The object whose type's full name should be included in the exception.</param>
        /// <exception cref="ObjectDisposedException">Always thrown.</exception>
        [DoesNotReturn]
        private static void ThrowObjectDisposedException(object instance) =>
            throw new ObjectDisposedException(instance?.GetType().FullName);

        /// <summary>Throws an <see cref="ObjectDisposedException"/> with the object name derived from the specified <paramref name="type"/>.</summary>
        /// <param name="type">The type whose full name should be included in the exception.</param>
        /// <exception cref="ObjectDisposedException">Always thrown.</exception>
        [DoesNotReturn]
        private static void ThrowObjectDisposedException(Type type) =>
            throw new ObjectDisposedException(type?.FullName);
    }
}
#endif
