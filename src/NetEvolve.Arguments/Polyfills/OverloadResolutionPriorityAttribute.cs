#if !NET9_0_OR_GREATER

#pragma warning disable IDE0130, NE0002 // Namespace does not match folder structure
namespace System.Runtime.CompilerServices;

#pragma warning restore IDE0130, NE0002 // Namespace does not match folder structure

using Diagnostics;
using Diagnostics.CodeAnalysis;

/// <summary>
/// Specifies the priority of a member in overload resolution. When unspecified, the default priority is 0.
/// </summary>
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
[AttributeUsage(
    AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property,
    AllowMultiple = false,
    Inherited = false
)]
internal sealed class OverloadResolutionPriorityAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OverloadResolutionPriorityAttribute"/> class.
    /// </summary>
    /// <param name="priority">The priority of the attributed member. Higher numbers are prioritized, lower numbers are deprioritized.</param>
    public OverloadResolutionPriorityAttribute(int priority) => Priority = priority;

    /// <summary>
    /// Gets the priority of the member.
    /// </summary>
    public int Priority { get; }
}

#else
using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(OverloadResolutionPriorityAttribute))]
#endif
