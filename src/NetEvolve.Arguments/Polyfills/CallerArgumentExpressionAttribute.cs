#if !NETCOREAPP3_0_OR_GREATER

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Runtime.CompilerServices;

#pragma warning restore IDE0130 // Namespace does not match folder structure

using Diagnostics;
using Diagnostics.CodeAnalysis;

/// <summary>
/// Indicates that a parameter captures the expression passed for another parameter as a string.
/// </summary>
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class CallerArgumentExpressionAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CallerArgumentExpressionAttribute"/> class.
    /// </summary>
    public CallerArgumentExpressionAttribute(string parameterName) => ParameterName = parameterName;

    /// <summary>
    /// Gets the name of the parameter whose expression should be captured as a string.
    /// </summary>
    public string ParameterName { get; }
}

#else
using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(CallerArgumentExpressionAttribute))]
#endif
