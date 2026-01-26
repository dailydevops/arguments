# Arguments

[![License](https://img.shields.io/github/license/dailydevops/arguments.svg)](https://github.com/dailydevops/arguments/blob/main/LICENSE)
[![Build Status](https://img.shields.io/github/actions/workflow/status/dailydevops/arguments/ci.yml?branch=main)](https://github.com/dailydevops/arguments/actions)
[![NuGet](https://img.shields.io/nuget/v/NetEvolve.Arguments.svg)](https://www.nuget.org/packages/NetEvolve.Arguments/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/NetEvolve.Arguments.svg)](https://www.nuget.org/packages/NetEvolve.Arguments/)
[![Contributors](https://img.shields.io/github/contributors/dailydevops/arguments.svg)](https://github.com/dailydevops/arguments/graphs/contributors)

A universal polyfill library that provides modern `ArgumentNullException.ThrowIf*` and `ArgumentException.ThrowIf*` helper methods across all .NET runtimes (.NET Standard 2.0+, .NET Framework 4.6.2+, .NET 6.0+), enabling consistent argument validation patterns regardless of target framework version.

## Overview

NetEvolve.Arguments brings modern .NET argument validation APIs to legacy frameworks, allowing you to write consistent defensive programming code across all supported .NET platforms. The library polyfills the `ThrowIfNull`, `ThrowIfNullOrEmpty`, `ThrowIfNullOrWhiteSpace`, and comparison-based validation methods that were introduced in later .NET versions, making them available to projects targeting older frameworks.

This solution provides a single, focused library designed to:

- **Enable modern code patterns**: Write modern argument validation code that works on .NET Framework 4.6.2 through .NET 10.0
- **Maintain consistency**: Use the same API surface across all target frameworks without conditional compilation
- **Simplify maintenance**: Replace verbose manual null checks and validation logic with concise, expressive helper methods
- **Improve code quality**: Apply defensive programming practices consistently throughout your codebase

## Projects

### Core Library

- **NetEvolve.Arguments** - The main polyfill library providing argument validation helper methods for:
  - Null checking (`ThrowIfNull`)
  - Empty/whitespace validation (`ThrowIfNullOrEmpty`, `ThrowIfNullOrWhiteSpace`)
  - Equality validation (`ThrowIfEqual`, `ThrowIfNotEqual`)
  - Comparison validation (`ThrowIfGreaterThan`, `ThrowIfGreaterThanOrEqual`, `ThrowIfLessThan`, `ThrowIfLessThanOrEqual`)
  - Object disposal validation (`ObjectDisposedException.ThrowIf`)

### Tests

- **NetEvolve.Arguments.Tests.Unit** - Comprehensive unit tests covering all validation methods across all target frameworks

## Features

- **Universal compatibility** - Supports .NET Standard 2.0+, .NET Framework 4.6.2, 4.7.2, 4.8, 4.8.1, and .NET 6.0 through 10.0
- **Modern API surface** - Provides the same helper methods available in .NET 6+ to all target frameworks
- **Zero overhead** - On frameworks where native implementations exist, the polyfills are compiled out
- **Type-safe validation** - Generic and specialized overloads for common scenarios including pointers and spans
- **Performance optimized** - Minimal allocations and optimized code paths for each target framework
- **Comprehensive testing** - Extensive unit test coverage across all supported frameworks

## Getting Started

### Prerequisites

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) or higher for building the solution
- [Git](https://git-scm.com/) for version control
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/) (recommended)

### Installation

Add the NuGet package to your project:

```bash
dotnet add package NetEvolve.Arguments
```

Or via Package Manager Console:

```powershell
Install-Package NetEvolve.Arguments
```

Or add directly to your `.csproj` file:

```xml
<ItemGroup>
  <PackageReference Include="NetEvolve.Arguments" />
</ItemGroup>
```

> [!NOTE]
> This project uses Centralized Package Version Management. When adding the package reference, omit the `Version` attribute - versions are managed centrally in [Directory.Packages.props](https://github.com/dailydevops/arguments/blob/main/Directory.Packages.props).

### Usage Examples

**Null validation:**

```csharp
using System;

public void ProcessData(string data)
{
    ArgumentNullException.ThrowIfNull(data);
    // Process data safely
}
```

**Empty/whitespace validation:**

```csharp
using System;

public void ValidateInput(string input)
{
    ArgumentException.ThrowIfNullOrEmpty(input);
    ArgumentException.ThrowIfNullOrWhiteSpace(input);
    // Input is guaranteed to have content
}
```

**Comparison validation:**

```csharp
using System;

public void SetTimeout(int milliseconds)
{
    ArgumentOutOfRangeException.ThrowIfNegative(milliseconds);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(milliseconds, 60000);
    // Timeout is between 0 and 60000
}
```

**Object disposal validation:**

```csharp
using System;

public class MyDisposable : IDisposable
{
    private bool _disposed;

    public void DoWork()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        // Safe to perform work
    }

    public void Dispose() => _disposed = true;
}
```

## Development

### Building

Clone the repository and build the solution:

```bash
git clone https://github.com/dailydevops/arguments.git
cd arguments
dotnet restore
dotnet build
```

### Running Tests

Run all tests across all target frameworks:

```bash
dotnet test
```

Run tests for a specific project:

```bash
dotnet test tests/NetEvolve.Arguments.Tests.Unit
```

### Code Formatting

This project uses [CSharpier](https://csharpier.com/) for consistent code formatting:

```bash
dotnet csharpier format .
```

Code formatting is automatically enforced during builds via the `CSharpier.MSBuild` package.

### Project Structure

```txt
src/
└── NetEvolve.Arguments/                            # Main polyfill library
    ├── Argument*.cs                                # Obsolete implementations
    ├── ArgumentExceptionPolyfill.cs                # Polyfill implementations
    ├── ArgumentNullExceptionPolyfills.cs           # Polyfill implementations
    ├── ArgumentOutOfRangeExceptionPolyfills.cs     # Polyfill implementations
    └── ObjectDisposedExceptionPolyfills.cs         # Polyfill implementations

tests/
└── NetEvolve.Arguments.Tests.Unit/                 # Comprehensive unit tests

decisions/                                          # Architecture Decision Records (ADRs)
templates/                                          # Documentation templates
```

## Architecture

This library follows a polyfill architecture pattern:

- **Conditional compilation**: Uses preprocessor directives to provide native implementations where available and polyfills where needed
- **Framework detection**: Automatically detects target framework capabilities at compile time
- **Zero-overhead abstraction**: On modern frameworks, the polyfills compile to simple pass-through calls or are eliminated entirely
- **Backward compatibility**: Ensures older frameworks receive functionally equivalent implementations

For detailed architectural decisions, see:

- [Centralized Package Version Management](https://github.com/dailydevops/arguments/blob/main/decisions/2025-07-10-centralized-package-version-management.md)
- [Conventional Commits](https://github.com/dailydevops/arguments/blob/main/decisions/2025-07-10-conventional-commits.md)
- [GitVersion for Automated Semantic Versioning](https://github.com/dailydevops/arguments/blob/main/decisions/2025-07-10-gitversion-automated-semantic-versioning.md)
- [.NET 10 and C# 13 Adoption](https://github.com/dailydevops/arguments/blob/main/decisions/2025-07-11-dotnet-10-csharp-13-adoption.md)
- [English as Project Language](https://github.com/dailydevops/arguments/blob/main/decisions/2025-07-11-english-as-project-language.md)
- [DateTimeOffset and TimeProvider Usage](https://github.com/dailydevops/arguments/blob/main/decisions/2026-01-21-datetimeoffset-and-timeprovider-usage.md)
- [All Architecture Decision Records](https://github.com/dailydevops/arguments/tree/main/decisions/)

## Contributing

We welcome contributions from the community! Please read our [Contributing Guidelines](https://github.com/dailydevops/arguments/blob/main/CONTRIBUTING.md) before submitting a pull request.

Key points:

- Follow the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) format for commit messages
- Write tests for new functionality across all relevant target frameworks
- Follow existing code style and conventions (enforced by analyzers and CSharpier)
- Update documentation as needed
- Ensure all tests pass on all target frameworks before submitting

### Code of Conduct

This project adheres to the Contributor Covenant [Code of Conduct](https://github.com/dailydevops/arguments/blob/main/CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to info@daily-devops.net.

### Documentation

- **[Architecture Decision Records](https://github.com/dailydevops/arguments/tree/main/decisions/)** - Detailed architectural decisions and rationale
- **[Contributing Guidelines](https://github.com/dailydevops/arguments/blob/main/CONTRIBUTING.md)** - How to contribute to this project
- **[Code of Conduct](https://github.com/dailydevops/arguments/blob/main/CODE_OF_CONDUCT.md)** - Community standards and expectations
- **[License](https://github.com/dailydevops/arguments/blob/main/LICENSE)** - Project licensing information (MIT License)
- **[Release Notes](https://github.com/dailydevops/arguments/releases/)** - Version history and changelog

### Versioning

This project uses [GitVersion](https://gitversion.net/) for automated semantic versioning based on Git history and [Conventional Commits](https://www.conventionalcommits.org/). Version numbers are automatically calculated during the build process:

- **Major version** (1.0.0 → 2.0.0): Breaking changes indicated by `!` or `BREAKING CHANGE:` footer in commit messages
- **Minor version** (1.0.0 → 1.1.0): New features added via `feat:` commits
- **Patch version** (1.0.0 → 1.0.1): Bug fixes and maintenance via `fix:`, `chore:`, `docs:`, etc.

For more details, see [GitVersion for Automated Semantic Versioning](https://github.com/dailydevops/arguments/blob/main/decisions/2025-07-10-gitversion-automated-semantic-versioning.md).

### Support

- **Issues**: Report bugs or request features on [GitHub Issues](https://github.com/dailydevops/arguments/issues)
- **Documentation**: Read the full documentation in this repository
- **NuGet**: Download the package from [NuGet.org](https://www.nuget.org/packages/NetEvolve.Arguments/)

## License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/dailydevops/arguments/blob/main/LICENSE) file for details.

---

> [!NOTE]
> **Made with ❤️ by the NetEvolve Team**
>
> Visit us at [https://www.daily-devops.net](https://www.daily-devops.net) for more information about our services and solutions.
