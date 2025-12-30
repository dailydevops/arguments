#if !NETCOREAPP3_0_OR_GREATER && !NETSTANDARD2_1_OR_GREATER

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Diagnostics.CodeAnalysis;

#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
///   Specifies that the method will not return if the associated <see cref="bool"/>
///   parameter is passed the specified value.
/// </summary>
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class DoesNotReturnIfAttribute : Attribute
{
    /// <summary>
    ///   Gets the condition parameter value.
    ///   Code after the method is considered unreachable by diagnostics if the argument
    ///   to the associated parameter matches this value.
    /// </summary>
    public bool ParameterValue { get; }

    /// <summary>
    ///   Initializes a new instance of the <see cref="DoesNotReturnIfAttribute"/>
    ///   class with the specified parameter value.
    /// </summary>
    public DoesNotReturnIfAttribute(bool parameterValue) => ParameterValue = parameterValue;
}
#else
using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(System.Diagnostics.CodeAnalysis.DoesNotReturnIfAttribute))]
#endif
