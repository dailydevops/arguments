# NetEvolve.Arguments

[![License](https://img.shields.io/github/license/dailydevops/arguments)](LICENSE)
[![Build Status](https://github.com/dailydevops/arguments/actions/workflows/build.yml/badge.svg)](https://github.com/dailydevops/arguments/actions)

A comprehensive library providing backward-compatible argument validation helper methods (`ThrowIf*`) for .NET projects targeting multiple framework versions. This library enables modern argument validation patterns across legacy and current .NET runtimes, ensuring code consistency and maintainability.

## Overview

Modern .NET versions (starting with .NET 6) introduced streamlined argument validation methods such as `ArgumentNullException.ThrowIfNull` and `ArgumentOutOfRangeException.ThrowIfEqual`. However, projects targeting multiple frameworks or older .NET versions cannot utilize these convenient methods without conditional compilation or duplicated validation logic.

**NetEvolve.Arguments** bridges this gap by providing polyfilled implementations of these modern validation methods, allowing developers to write consistent, maintainable argument validation code regardless of the target framework.

## Key Features

- **Multi-Framework Support**: Compatible with .NET Standard 2.0, .NET 7.0, .NET 8.0, .NET 9.0, and .NET 10.0
- **Zero Runtime Overhead**: Uses conditional compilation to delegate to native implementations where available
- **Drop-in Replacement**: Identical API signatures to native .NET implementations
- **Type-Safe**: Fully generic implementations with proper type constraints
- **Comprehensive Coverage**: Includes null checks, range validations, and equality comparisons

## Installation

Install the package via NuGet Package Manager:

```bash
dotnet add package NetEvolve.Arguments
```

Or via the Package Manager Console:

```powershell
Install-Package NetEvolve.Arguments
```

## Usage

Import the namespace in your code:

```csharp
using NetEvolve.Arguments;
```

Then use the validation methods just as you would with native .NET implementations:

```csharp
public void ProcessData(string data, int count)
{
    Argument.ThrowIfNullOrWhiteSpace(data);
    Argument.ThrowIfLessThan(count, 1);
    
    // Your implementation
}
```

## Available Methods

### Null Validation

#### `Argument.ThrowIfNull(object?, string?)`
Throws an `ArgumentNullException` if the argument is `null`.

**Replacement for**: [`ArgumentNullException.ThrowIfNull(object, string)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception.throwifnull) (introduced in .NET 6)

**Example**:
```csharp
public void Process(object data)
{
    Argument.ThrowIfNull(data);
}
```

#### `Argument.ThrowIfNull(void*, string?)`
Throws an `ArgumentNullException` if the pointer argument is `null`.

**Replacement for**: [`ArgumentNullException.ThrowIfNull(void*, string)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception.throwifnull?view=net-8.0#system-argumentnullexception-throwifnull(system-void*-system-string)) (introduced in .NET 7)

#### `Argument.ThrowIfNullOrEmpty(string?, string?)`
Throws an `ArgumentNullException` if the argument is `null`, or an `ArgumentException` if the argument is an empty string.

**Replacement for**: [`ArgumentException.ThrowIfNullOrEmpty(string, string)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception.throwifnullorempty) (introduced in .NET 7)

**Example**:
```csharp
public void Process(string name)
{
    Argument.ThrowIfNullOrEmpty(name);
}
```

#### `Argument.ThrowIfNullOrEmpty<T>(IEnumerable<T>?, string?)`
Throws an `ArgumentNullException` if the argument is `null`, or an `ArgumentException` if the collection is empty.

**Note**: This is a custom extension method not present in the base .NET framework, providing convenient collection validation.

**Example**:
```csharp
public void Process(IEnumerable<int> items)
{
    Argument.ThrowIfNullOrEmpty(items);
}
```

#### `Argument.ThrowIfNullOrWhiteSpace(string?, string?)`
Throws an `ArgumentNullException` if the argument is `null`, or an `ArgumentException` if the argument is empty or contains only white-space characters.

**Replacement for**: [`ArgumentException.ThrowIfNullOrWhiteSpace(string, string)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception.throwifnullorwhitespace) (introduced in .NET 8)

**Example**:
```csharp
public void Process(string description)
{
    Argument.ThrowIfNullOrWhiteSpace(description);
}
```

### Range Validation

#### `Argument.ThrowIfEqual<T>(T, T, string?)`
Throws an `ArgumentOutOfRangeException` if the first argument is equal to the second argument.

**Replacement for**: [`ArgumentOutOfRangeException.ThrowIfEqual<T>(T, T, string)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifequal) (introduced in .NET 8)

**Example**:
```csharp
public void SetValue(int value)
{
    Argument.ThrowIfEqual(value, 0); // Value must not be zero
}
```

#### `Argument.ThrowIfNotEqual<T>(T, T, string?)`
Throws an `ArgumentOutOfRangeException` if the first argument is not equal to the second argument.

**Replacement for**: [`ArgumentOutOfRangeException.ThrowIfNotEqual<T>(T, T, string)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifnotequal) (introduced in .NET 8)

#### `Argument.ThrowIfGreaterThan<T>(T, T, string?)`
Throws an `ArgumentOutOfRangeException` if the first argument is greater than the second argument.

**Replacement for**: [`ArgumentOutOfRangeException.ThrowIfGreaterThan<T>(T, T, string)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifgreaterthan) (introduced in .NET 8)

**Example**:
```csharp
public void SetAge(int age)
{
    Argument.ThrowIfGreaterThan(age, 150); // Age must be 150 or less
}
```

#### `Argument.ThrowIfGreaterThanOrEqual<T>(T, T, string?)`
Throws an `ArgumentOutOfRangeException` if the first argument is greater than or equal to the second argument.

**Replacement for**: [`ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual<T>(T, T, string)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifgreaterthanorequal) (introduced in .NET 8)

**Example**:
```csharp
public void SetCount(int count, int maximum)
{
    Argument.ThrowIfGreaterThanOrEqual(count, maximum); // Count must be less than maximum
}
```

#### `Argument.ThrowIfLessThan<T>(T, T, string?)`
Throws an `ArgumentOutOfRangeException` if the first argument is less than the second argument.

**Replacement for**: [`ArgumentOutOfRangeException.ThrowIfLessThan<T>(T, T, string)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwiflessthan) (introduced in .NET 8)

**Example**:
```csharp
public void SetCount(int count)
{
    Argument.ThrowIfLessThan(count, 1); // Count must be at least 1
}
```

#### `Argument.ThrowIfLessThanOrEqual<T>(T, T, string?)`
Throws an `ArgumentOutOfRangeException` if the first argument is less than or equal to the second argument.

**Replacement for**: [`ArgumentOutOfRangeException.ThrowIfLessThanOrEqual<T>(T, T, string)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwiflessthanorequal) (introduced in .NET 8)

**Example**:
```csharp
public void SetMinimum(int value, int threshold)
{
    Argument.ThrowIfLessThanOrEqual(value, threshold); // Value must be greater than threshold
}
```

## Framework Compatibility

| Target Framework | Status | Notes |
|-----------------|--------|-------|
| .NET Standard 2.0 | ✅ Supported | Full polyfill implementations |
| .NET 7.0 | ✅ Supported | Partial native delegation |
| .NET 8.0 | ✅ Supported | Delegates to native implementations where available |
| .NET 9.0 | ✅ Supported | Full native delegation |
| .NET 10.0 | ✅ Supported | Full native delegation |

## Contributing

Contributions are welcome! Please feel free to submit issues, fork the repository, and create pull requests.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Related Resources

- [Official .NET API Documentation](https://learn.microsoft.com/en-us/dotnet/api/)
- [Argument Validation Best Practices](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/exceptions/)
- [Project Repository](https://github.com/dailydevops/arguments)