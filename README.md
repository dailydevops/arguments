# NetEvolve.Arguments

[![License](https://img.shields.io/github/license/dailydevops/arguments)](LICENSE)
[![Build Status](https://github.com/dailydevops/arguments/actions/workflows/build.yml/badge.svg)](https://github.com/dailydevops/arguments/actions)

A comprehensive library providing backward-compatible argument validation helper methods (`ThrowIf*`) for .NET projects targeting multiple framework versions. This library enables modern argument validation patterns across legacy and current .NET runtimes, ensuring code consistency and maintainability.

## Overview

Modern .NET versions (starting with .NET 6) introduced streamlined argument validation methods such as `ArgumentNullException.ThrowIfNull` and `ArgumentOutOfRangeException.ThrowIfEqual`. However, projects targeting multiple frameworks or older .NET versions cannot utilize these convenient methods without conditional compilation or duplicated validation logic.

**NetEvolve.Arguments** bridges this gap by providing full polyfill implementations via extension methods on `ArgumentNullException`, `ArgumentException`, and `ArgumentOutOfRangeException`. These polyfills enable the use of modern .NET API patterns across all supported frameworks, allowing developers to write consistent, maintainable argument validation code regardless of the target framework.

### Polyfill Architecture

The library provides polyfills through three main extension classes:

- **`ArgumentNullExceptionPolyfills`**: Extends `ArgumentNullException` with `ThrowIfNull` methods
- **`ArgumentExceptionPolyfills`**: Extends `ArgumentException` with `ThrowIfNullOrEmpty` and `ThrowIfNullOrWhiteSpace` methods
- **`ArgumentOutOfRangeExceptionPolyfills`**: Extends `ArgumentOutOfRangeException` with range validation methods (`ThrowIfZero`, `ThrowIfNegative`, `ThrowIfEqual`, comparison methods, etc.)

These polyfills are conditionally compiled and only active when targeting frameworks that don't provide the native implementations, ensuring zero overhead on modern .NET versions.

## Key Features

- **Multi-Framework Support**: Compatible with .NET Standard 2.0, .NET 6.0-10.0, and .NET Framework 4.7.2-4.8.1 (on Windows)
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

Simply use the validation methods directly on the exception types, just as you would with native .NET 8+ implementations:

```csharp
public void ProcessData(string data, int count)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(data);
    ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);
    
    // Your implementation
}
```

The polyfills are automatically available through extension methods when targeting older frameworks. No additional using directives are needed since the polyfills reside in the `System` namespace.

## Available Methods

### Null Validation

#### `ArgumentNullException.ThrowIfNull(object?, string?)`
Throws an `ArgumentNullException` if the argument is `null`.

**Native API**: [`ArgumentNullException.ThrowIfNull`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception.throwifnull) (introduced in .NET 6)

**Polyfill availability**: .NET Standard 2.0, .NET Framework 4.7.2-4.8.1

**Example**:
```csharp
public void Process(object data)
{
    ArgumentNullException.ThrowIfNull(data);
}
```

#### `ArgumentNullException.ThrowIfNull(void*, string?)`
Throws an `ArgumentNullException` if the pointer argument is `null`.

**Native API**: [`ArgumentNullException.ThrowIfNull(void*)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception.throwifnull?view=net-10.0#system-argumentnullexception-throwifnull(system-void*-system-string)) (introduced in .NET 7)

**Polyfill availability**: .NET Standard 2.0, .NET Framework 4.7.2-4.8.1, .NET 6.0

**Example**:
```csharp
public unsafe void Process(void* pointer)
{
    ArgumentNullException.ThrowIfNull(pointer);
}
```

#### `ArgumentException.ThrowIfNullOrEmpty(string?, string?)`
Throws an `ArgumentNullException` if the argument is `null`, or an `ArgumentException` if the argument is an empty string.

**Native API**: [`ArgumentException.ThrowIfNullOrEmpty`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception.throwifnullorempty) (introduced in .NET 7)

**Polyfill availability**: .NET Standard 2.0, .NET Framework 4.7.2-4.8.1, .NET 6.0

**Example**:
```csharp
public void Process(string name)
{
    ArgumentException.ThrowIfNullOrEmpty(name);
}
```

#### `ArgumentException.ThrowIfNullOrEmpty<T>(IEnumerable<T>?, string?)`
Throws an `ArgumentNullException` if the argument is `null`, or an `ArgumentException` if the collection is empty.

**Note**: This is a custom extension method not present in the native .NET framework, providing convenient collection validation.

**Availability**: All supported frameworks

**Example**:
```csharp
public void Process(IEnumerable<int> items)
{
    ArgumentException.ThrowIfNullOrEmpty(items);
}
```

#### `ArgumentException.ThrowIfNullOrWhiteSpace(string?, string?)`
Throws an `ArgumentNullException` if the argument is `null`, or an `ArgumentException` if the argument is empty or contains only white-space characters.

**Native API**: [`ArgumentException.ThrowIfNullOrWhiteSpace`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception.throwifnullorwhitespace) (introduced in .NET 8)

**Polyfill availability**: .NET Standard 2.0, .NET Framework 4.7.2-4.8.1, .NET 6.0, .NET 7.0

**Example**:
```csharp
public void Process(string description)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(description);
}
```

### Range Validation

#### `ArgumentOutOfRangeException.ThrowIfZero<T>(T, string?)`
Throws an `ArgumentOutOfRangeException` if the argument is zero.

**Native API**: [`ArgumentOutOfRangeException.ThrowIfZero`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifzero) (introduced in .NET 8)

**Polyfill availability**: .NET Standard 2.0, .NET Framework 4.7.2-4.8.1, .NET 6.0, .NET 7.0

**Example**:
```csharp
public void SetDivisor(int divisor)
{
    ArgumentOutOfRangeException.ThrowIfZero(divisor);
}
```

#### `ArgumentOutOfRangeException.ThrowIfNegative<T>(T, string?)`
Throws an `ArgumentOutOfRangeException` if the argument is negative.

**Native API**: [`ArgumentOutOfRangeException.ThrowIfNegative`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifnegative) (introduced in .NET 8)

**Polyfill availability**: .NET Standard 2.0, .NET Framework 4.7.2-4.8.1, .NET 6.0, .NET 7.0

**Example**:
```csharp
public void SetCount(int count)
{
    ArgumentOutOfRangeException.ThrowIfNegative(count);
}
```

#### `ArgumentOutOfRangeException.ThrowIfNegativeOrZero<T>(T, string?)`
Throws an `ArgumentOutOfRangeException` if the argument is negative or zero.

**Native API**: [`ArgumentOutOfRangeException.ThrowIfNegativeOrZero`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifnegativeorzero) (introduced in .NET 8)

**Polyfill availability**: .NET Standard 2.0, .NET Framework 4.7.2-4.8.1, .NET 6.0, .NET 7.0

**Example**:
```csharp
public void SetQuantity(int quantity)
{
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
}
```

#### `ArgumentOutOfRangeException.ThrowIfEqual<T>(T, T, string?)`
Throws an `ArgumentOutOfRangeException` if the first argument is equal to the second argument.

**Native API**: [`ArgumentOutOfRangeException.ThrowIfEqual`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifequal) (introduced in .NET 8)

**Polyfill availability**: .NET Standard 2.0, .NET Framework 4.7.2-4.8.1, .NET 6.0, .NET 7.0

**Example**:
```csharp
public void SetValue(int value)
{
    ArgumentOutOfRangeException.ThrowIfEqual(value, 0); // Value must not be zero
}
```

#### `ArgumentOutOfRangeException.ThrowIfNotEqual<T>(T, T, string?)`
Throws an `ArgumentOutOfRangeException` if the first argument is not equal to the second argument.

**Native API**: [`ArgumentOutOfRangeException.ThrowIfNotEqual`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifnotequal) (introduced in .NET 8)

**Polyfill availability**: .NET Standard 2.0, .NET Framework 4.7.2-4.8.1, .NET 6.0, .NET 7.0

**Example**:
```csharp
public void ValidateConstant(int value)
{
    ArgumentOutOfRangeException.ThrowIfNotEqual(value, 42); // Value must be exactly 42
}
```

#### `ArgumentOutOfRangeException.ThrowIfGreaterThan<T>(T, T, string?)`
Throws an `ArgumentOutOfRangeException` if the first argument is greater than the second argument.

**Native API**: [`ArgumentOutOfRangeException.ThrowIfGreaterThan`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifgreaterthan) (introduced in .NET 8)

**Polyfill availability**: .NET Standard 2.0, .NET Framework 4.7.2-4.8.1, .NET 6.0, .NET 7.0

**Example**:
```csharp
public void SetAge(int age)
{
    ArgumentOutOfRangeException.ThrowIfGreaterThan(age, 150); // Age must be 150 or less
}
```

#### `ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual<T>(T, T, string?)`
Throws an `ArgumentOutOfRangeException` if the first argument is greater than or equal to the second argument.

**Native API**: [`ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifgreaterthanorequal) (introduced in .NET 8)

**Polyfill availability**: .NET Standard 2.0, .NET Framework 4.7.2-4.8.1, .NET 6.0, .NET 7.0

**Example**:
```csharp
public void SetCount(int count, int maximum)
{
    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(count, maximum); // Count must be less than maximum
}
```

#### `ArgumentOutOfRangeException.ThrowIfLessThan<T>(T, T, string?)`
Throws an `ArgumentOutOfRangeException` if the first argument is less than the second argument.

**Native API**: [`ArgumentOutOfRangeException.ThrowIfLessThan`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwiflessthan) (introduced in .NET 8)

**Polyfill availability**: .NET Standard 2.0, .NET Framework 4.7.2-4.8.1, .NET 6.0, .NET 7.0

**Example**:
```csharp
public void SetCount(int count)
{
    ArgumentOutOfRangeException.ThrowIfLessThan(count, 1); // Count must be at least 1
}
```

#### `ArgumentOutOfRangeException.ThrowIfLessThanOrEqual<T>(T, T, string?)`
Throws an `ArgumentOutOfRangeException` if the first argument is less than or equal to the second argument.

**Native API**: [`ArgumentOutOfRangeException.ThrowIfLessThanOrEqual`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwiflessthanorequal) (introduced in .NET 8)

**Polyfill availability**: .NET Standard 2.0, .NET Framework 4.7.2-4.8.1, .NET 6.0, .NET 7.0

**Example**:
```csharp
public void SetMinimum(int value, int threshold)
{
    ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, threshold); // Value must be greater than threshold
}
```

## Framework Compatibility

| Target Framework | Status | Notes |
|-----------------|--------|-------|
| .NET Standard 2.0 | ✅ Supported | Full polyfill implementations |
| .NET Framework 4.7.2 | ✅ Supported | Windows only, full polyfill implementations |
| .NET Framework 4.8 | ✅ Supported | Windows only, full polyfill implementations |
| .NET Framework 4.8.1 | ✅ Supported | Windows only, full polyfill implementations |
| .NET 6.0 | ✅ Supported | Delegates to native implementations where available |
| .NET 7.0 | ✅ Supported | Delegates to native implementations where available |
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