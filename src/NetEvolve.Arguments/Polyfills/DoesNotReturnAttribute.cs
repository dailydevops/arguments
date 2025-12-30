#if !NETCOREAPP3_0_OR_GREATER && !NETSTANDARD2_1_OR_GREATER

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Diagnostics.CodeAnalysis;

#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
///   Specifies that a method that will never return under any circumstance.
/// </summary>
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
[AttributeUsage(validOn: AttributeTargets.Method, Inherited = false)]
internal sealed class DoesNotReturnAttribute : Attribute;
#else
using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(System.Diagnostics.CodeAnalysis.DoesNotReturnAttribute))]
#endif
