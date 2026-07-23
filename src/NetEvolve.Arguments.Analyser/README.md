# NetEvolve.Arguments.Analyser

[![NuGet Version](https://img.shields.io/nuget/v/NetEvolve.Arguments.Analyser.svg)](https://www.nuget.org/packages/NetEvolve.Arguments.Analyser/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/NetEvolve.Arguments.Analyser.svg)](https://www.nuget.org/packages/NetEvolve.Arguments.Analyser/)
[![License](https://img.shields.io/github/license/dailydevops/arguments.svg)](https://github.com/dailydevops/arguments/blob/main/LICENSE)

A Roslyn analyzer and code-fix package that promotes usage of [NetEvolve.Arguments](https://www.nuget.org/packages/NetEvolve.Arguments/) throw-helper APIs, rewriting manual argument-validation `if` blocks on every target framework — including .NET Standard 2.0 and .NET Framework 4.7.2+, which predate the built-in `CA1510`/`CA1511`/`CA1512`/`CA1513` analyzers.

## Features

- **9 analyzer rules (NEA0001-NEA0009)** - Covers null checks, string/collection validation, numeric ranges, `Guid`, and disposed-object checks
- **Works everywhere** - Unlike the built-in Roslyn rules, these analyzers fire regardless of target framework, since [NetEvolve.Arguments](https://www.nuget.org/packages/NetEvolve.Arguments/) polyfills the throw-helper APIs on frameworks that predate them
- **No duplicate diagnostics** - Rules that mirror a built-in CA rule (NEA0001, NEA0002, NEA0003, NEA0005) automatically stay silent once the compilation already exposes the real BCL member, so CA1510/CA1511/CA1512/CA1513 take over seamlessly
- **One-click code fixes** - Every diagnostic ships with a code fix that rewrites the offending `if` block in place, usable per-occurrence or via Fix All in Document/Project/Solution scope
- **Recognizes common variants** - Null checks match `is null`, `==`, `ReferenceEquals`, negated forms, and null-coalescing throws (`arg ?? throw ...`)

## Installation

### NuGet Package Manager

```powershell
Install-Package NetEvolve.Arguments.Analyser
```

### .NET CLI

```bash
dotnet add package NetEvolve.Arguments.Analyser
```

### PackageReference

```xml
<PackageReference Include="NetEvolve.Arguments.Analyser" PrivateAssets="all" />
```

## Quick Start

```csharp
using System;

public class UserService
{
    public void CreateUser(string username, int age)
    {
        // NEA0001 suggests replacing this with ArgumentNullException.ThrowIfNull(username)
        if (username is null)
        {
            throw new ArgumentNullException(nameof(username));
        }

        // NEA0003 suggests replacing this with ArgumentOutOfRangeException.ThrowIfNegative(age)
        if (age < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(age));
        }
    }
}
```

## Rules

Each rule has a dedicated documentation page with the full list of recognized shapes, examples, and suppression instructions.

| Rule                                                                                    | Title                                               | Mirrors                                                                                      |
| --------------------------------------------------------------------------------------- | --------------------------------------------------- | -------------------------------------------------------------------------------------------- |
| [NEA0001](https://github.com/dailydevops/arguments/blob/main/docs/analysers/NEA0001.md) | Use ArgumentNullException.ThrowIfNull               | [CA1510](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1510) |
| [NEA0002](https://github.com/dailydevops/arguments/blob/main/docs/analysers/NEA0002.md) | Use ArgumentException throw helper                  | [CA1511](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1511) |
| [NEA0003](https://github.com/dailydevops/arguments/blob/main/docs/analysers/NEA0003.md) | Use ArgumentOutOfRangeException throw helper        | [CA1512](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1512) |
| [NEA0004](https://github.com/dailydevops/arguments/blob/main/docs/analysers/NEA0004.md) | Use ArgumentException.ThrowIfDefault                | _(NetEvolve.Arguments-only, no CA equivalent)_                                               |
| [NEA0005](https://github.com/dailydevops/arguments/blob/main/docs/analysers/NEA0005.md) | Use ObjectDisposedException.ThrowIf                 | [CA1513](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1513) |
| [NEA0006](https://github.com/dailydevops/arguments/blob/main/docs/analysers/NEA0006.md) | Use ArgumentException string-length throw helper    | _(NetEvolve.Arguments-only, no CA equivalent)_                                               |
| [NEA0007](https://github.com/dailydevops/arguments/blob/main/docs/analysers/NEA0007.md) | Use ArgumentException collection-count throw helper | _(NetEvolve.Arguments-only, no CA equivalent)_                                               |
| [NEA0008](https://github.com/dailydevops/arguments/blob/main/docs/analysers/NEA0008.md) | Use ArgumentException.ThrowIfContainsWhiteSpace     | _(NetEvolve.Arguments-only, no CA equivalent)_                                               |
| [NEA0009](https://github.com/dailydevops/arguments/blob/main/docs/analysers/NEA0009.md) | Use ArgumentException.ThrowIfEmptyGuid              | _(NetEvolve.Arguments-only, no CA equivalent)_                                               |

### Example: NEA0001 (null check)

```csharp
// Before
if (argument is null) throw new ArgumentNullException(nameof(argument));

// After
ArgumentNullException.ThrowIfNull(argument);
```

### Example: NEA0003 (range check)

```csharp
// Before
if (age < 0) throw new ArgumentOutOfRangeException(nameof(age));

// After
ArgumentOutOfRangeException.ThrowIfNegative(age);
```

### Example: NEA0007 (collection count)

```csharp
// Before
if (items.Count > 100) throw new ArgumentException(nameof(items));

// After
ArgumentException.ThrowIfCountGreaterThan(items, 100);
```

## Not covered

A few NetEvolve.Arguments throw-helpers don't have a dedicated rule, because their manual equivalents can be written in too many syntactically different ways to detect reliably without false negatives or false positives:

- `ArgumentException.ThrowIfNullOrEmpty` for collections (`IEnumerable<T>`/`ICollection<T>`/`IReadOnlyCollection<T>`/`T[]`)
- `ArgumentException.ThrowIfContainsDuplicates`
- `ArgumentOutOfRangeException.ThrowIfInPast`/`ThrowIfInFuture`

See the [open issues](https://github.com/dailydevops/arguments/issues) for tracking status.

## Requirements

- .NET Standard 2.0 or higher for the analyzed project
- Visual Studio 2022 17.8+, Rider, or any IDE/CLI supporting Roslyn source analyzers

## Related Packages

- [**NetEvolve.Arguments**](https://www.nuget.org/packages/NetEvolve.Arguments/) - The polyfill library this analyzer promotes usage of

## Documentation

For complete solution documentation, architecture decisions, and contributing guidelines, visit the [Arguments Repository](https://github.com/dailydevops/arguments).

## Contributing

Contributions are welcome! Please read the [Contributing Guidelines](https://github.com/dailydevops/arguments/blob/main/CONTRIBUTING.md) before submitting a pull request.

## Support

- **Issues**: Report bugs or request features on [GitHub Issues](https://github.com/dailydevops/arguments/issues)
- **Documentation**: Read the full documentation at [https://github.com/dailydevops/arguments](https://github.com/dailydevops/arguments)

## License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/dailydevops/arguments/blob/main/LICENSE) file for details.

---

> [!NOTE]
> **Made with ❤️ by the NetEvolve Team**
> Visit us at [https://www.daily-devops.net](https://www.daily-devops.net) for more information about our services and solutions.
