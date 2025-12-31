#if !NET5_0_OR_GREATER

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Runtime.CompilerServices;

#pragma warning restore IDE0130 // Namespace does not match folder structure

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using static AttributeTargets;

/// <summary>
/// Used to indicate to the compiler that a method should be called
/// in its containing module's initializer.
/// </summary>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.moduleinitializerattribute?view=net-10.0"/>
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
[AttributeUsage(validOn: Method, Inherited = false)]
internal sealed class ModuleInitializerAttribute : Attribute;
#else
using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(ModuleInitializerAttribute))]
#endif
