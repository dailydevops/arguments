#if !NET6_0_OR_GREATER
#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Diagnostics;

#pragma warning restore IDE0130 // Namespace does not match folder structure

using System.Diagnostics.CodeAnalysis;
using static AttributeTargets;

/// <summary>
/// Types and Methods attributed with StackTraceHidden will be omitted from the stack trace text shown in StackTrace.ToString()
/// and Exception.StackTrace
/// </summary>
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
[AttributeUsage(validOn: Class | Method | Constructor | Struct, Inherited = false)]
internal sealed class StackTraceHiddenAttribute : Attribute;
#else
using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(System.Diagnostics.StackTraceHiddenAttribute))]
#endif
