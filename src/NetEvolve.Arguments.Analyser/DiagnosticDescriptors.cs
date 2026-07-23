namespace NetEvolve.Arguments.Analyser;

using Microsoft.CodeAnalysis;

internal static class DiagnosticDescriptors
{
    private const string HelpLinkBase = "https://github.com/dailydevops/arguments/blob/main/docs/analysers";

    public static readonly DiagnosticDescriptor ThrowIfNull = new(
        id: "NEA0001",
        title: "Use ArgumentNullException.ThrowIfNull",
        messageFormat: "Use 'ArgumentNullException.ThrowIfNull({0})' instead of the explicit null-check and throw",
        category: "Maintainability",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "A null-check that throws ArgumentNullException can be replaced by the ArgumentNullException.ThrowIfNull throw-helper, which works on every target framework supported by NetEvolve.Arguments, including those that predate .NET 6.",
        helpLinkUri: $"{HelpLinkBase}/NEA0001.md"
    );

    public static readonly DiagnosticDescriptor ThrowIfNullOrEmpty = new(
        id: "NEA0002",
        title: "Use ArgumentException throw helper",
        messageFormat: "Use 'ArgumentException.{0}({1})' instead of the explicit check and throw",
        category: "Maintainability",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "A string.IsNullOrEmpty/IsNullOrWhiteSpace check that throws ArgumentException can be replaced by the ArgumentException.ThrowIfNullOrEmpty/ThrowIfNullOrWhiteSpace throw-helper, which works on every target framework supported by NetEvolve.Arguments, including those that predate .NET 8.",
        helpLinkUri: $"{HelpLinkBase}/NEA0002.md"
    );

    public static readonly DiagnosticDescriptor ThrowIfOutOfRange = new(
        id: "NEA0003",
        title: "Use ArgumentOutOfRangeException throw helper",
        messageFormat: "Use 'ArgumentOutOfRangeException.{0}({1})' instead of the explicit comparison and throw",
        category: "Maintainability",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "A comparison that throws ArgumentOutOfRangeException can be replaced by an ArgumentOutOfRangeException throw-helper, which works on every target framework supported by NetEvolve.Arguments, including those that predate .NET 8.",
        helpLinkUri: $"{HelpLinkBase}/NEA0003.md"
    );

    public static readonly DiagnosticDescriptor ThrowIfDefault = new(
        id: "NEA0004",
        title: "Use ArgumentException.ThrowIfDefault",
        messageFormat: "Use 'ArgumentException.ThrowIfDefault({0})' instead of the explicit default-value check and throw",
        category: "Maintainability",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "A default-value check that throws ArgumentException can be replaced by the ArgumentException.ThrowIfDefault throw-helper provided by NetEvolve.Arguments.",
        helpLinkUri: $"{HelpLinkBase}/NEA0004.md"
    );

    public static readonly DiagnosticDescriptor ThrowIfDisposed = new(
        id: "NEA0005",
        title: "Use ObjectDisposedException.ThrowIf",
        messageFormat: "Use 'ObjectDisposedException.ThrowIf({0}, this)' instead of the explicit disposed-check and throw",
        category: "Maintainability",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "A disposed-check that throws ObjectDisposedException can be replaced by the ObjectDisposedException.ThrowIf throw-helper, which works on every target framework supported by NetEvolve.Arguments, including those that predate .NET 7.",
        helpLinkUri: $"{HelpLinkBase}/NEA0005.md"
    );

    public static readonly DiagnosticDescriptor ThrowIfLength = new(
        id: "NEA0006",
        title: "Use ArgumentException string-length throw helper",
        messageFormat: "Use 'ArgumentException.{0}({1})' instead of the explicit string length comparison and throw",
        category: "Maintainability",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "A string length comparison that throws ArgumentException can be replaced by the ArgumentException.ThrowIfLengthGreaterThan/ThrowIfLengthLessThan/ThrowIfLengthOutOfRange throw-helper provided by NetEvolve.Arguments.",
        helpLinkUri: $"{HelpLinkBase}/NEA0006.md"
    );

    public static readonly DiagnosticDescriptor ThrowIfCount = new(
        id: "NEA0007",
        title: "Use ArgumentException collection-count throw helper",
        messageFormat: "Use 'ArgumentException.{0}({1})' instead of the explicit collection count comparison and throw",
        category: "Maintainability",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "A collection count comparison that throws ArgumentException can be replaced by the ArgumentException.ThrowIfCountGreaterThan/ThrowIfCountLessThan/ThrowIfCountOutOfRange throw-helper provided by NetEvolve.Arguments.",
        helpLinkUri: $"{HelpLinkBase}/NEA0007.md"
    );

    public static readonly DiagnosticDescriptor ThrowIfContainsWhiteSpace = new(
        id: "NEA0008",
        title: "Use ArgumentException.ThrowIfContainsWhiteSpace",
        messageFormat: "Use 'ArgumentException.ThrowIfContainsWhiteSpace({0})' instead of the explicit white-space check and throw",
        category: "Maintainability",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "A check for white-space characters that throws ArgumentException can be replaced by the ArgumentException.ThrowIfContainsWhiteSpace throw-helper provided by NetEvolve.Arguments.",
        helpLinkUri: $"{HelpLinkBase}/NEA0008.md"
    );

    public static readonly DiagnosticDescriptor ThrowIfEmptyGuid = new(
        id: "NEA0009",
        title: "Use ArgumentException.ThrowIfEmptyGuid",
        messageFormat: "Use 'ArgumentException.ThrowIfEmptyGuid({0})' instead of the explicit Guid.Empty check and throw",
        category: "Maintainability",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "A Guid.Empty check that throws ArgumentException can be replaced by the ArgumentException.ThrowIfEmptyGuid throw-helper provided by NetEvolve.Arguments.",
        helpLinkUri: $"{HelpLinkBase}/NEA0009.md"
    );
}
