# NetEvolve.Arguments

[![NuGet Version](https://img.shields.io/nuget/v/NetEvolve.Arguments.svg)](https://www.nuget.org/packages/NetEvolve.Arguments/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/NetEvolve.Arguments.svg)](https://www.nuget.org/packages/NetEvolve.Arguments/)
[![License](https://img.shields.io/github/license/dailydevops/arguments.svg)](https://github.com/dailydevops/arguments/blob/main/LICENSE)

A universal polyfill library that provides modern `ArgumentNullException.ThrowIf*` and `ArgumentException.ThrowIf*` helper methods across all .NET runtimes (.NET Standard 2.0+, .NET Framework 4.7.2+, .NET 6.0+), enabling consistent argument validation patterns regardless of target framework version.

## Features

- **Universal Compatibility** - Works seamlessly across .NET Framework 4.7.2+, .NET Standard 2.0+, and all modern .NET versions (6.0 through 10.0)
- **Modern API on Legacy Frameworks** - Brings .NET 8+ argument validation APIs to older framework versions through polyfills
- **Zero Learning Curve** - Uses the same API surface as modern .NET, making migration and cross-framework development effortless
- **Performance Optimized** - Aggressive inlining, stack trace hiding, and minimal overhead for validation operations
- **Comprehensive Validation Suite** - Over 40 validation methods covering nullability, ranges, collections, strings, dates, and more
- **CallerArgumentExpression Support** - Automatic parameter name capture for clearer exception messages
- **Type-Safe Generic Methods** - Strongly-typed validation with compile-time safety
- **Extended Validation Helpers** - Additional validation methods beyond the standard .NET API for common scenarios

## Installation

### NuGet Package Manager

```powershell
Install-Package NetEvolve.Arguments
```

### .NET CLI

```bash
dotnet add package NetEvolve.Arguments
```

### PackageReference

```xml
<PackageReference Include="NetEvolve.Arguments" />
```

## Quick Start

```csharp
using NetEvolve.Arguments;

public class UserService
{
    public void CreateUser(string username, string email, int age)
    {
        // Validate arguments with modern .NET 8+ API - works on all frameworks!
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentOutOfRangeException.ThrowIfNegative(age);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(age, 150);

        // Your logic here
    }
}
```

## Understanding the Polyfill Concept

NetEvolve.Arguments brings modern .NET 8+ argument validation APIs to older framework versions. This means:

- **On .NET 8+**: The library delegates to the built-in framework methods (zero overhead)
- **On .NET 6-7**: Provides polyfill implementations that match the .NET 8+ API exactly
- **On .NET Framework 4.7.2-4.8.1 and .NET Standard 2.0**: Full polyfill implementation

This allows you to write code once using modern patterns and have it work consistently across all supported frameworks.

## Usage

### Null Validation

#### ThrowIfNull

Validates that a reference or pointer is not null.

```csharp
public void ProcessData(object data)
{
    ArgumentNullException.ThrowIfNull(data);
    // data is guaranteed non-null here
}

// With unsafe pointers (requires unsafe context)
public unsafe void ProcessPointer(void* ptr)
{
    ArgumentNullException.ThrowIfNull(ptr);
    // ptr is guaranteed non-null here
}
```

### String Validation

#### ThrowIfNullOrEmpty

Validates that a string is neither null nor empty.

```csharp
public void SetUsername(string username)
{
    ArgumentException.ThrowIfNullOrEmpty(username);
    // username is guaranteed to have at least one character
}
```

#### ThrowIfNullOrWhiteSpace

Validates that a string is not null, empty, or whitespace-only.

```csharp
public void SetDescription(string description)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(description);
    // description is guaranteed to have non-whitespace content
}
```

#### ThrowIfLengthGreaterThan

Validates that a string does not exceed a maximum length.

```csharp
public void SetTitle(string title)
{
    ArgumentException.ThrowIfLengthGreaterThan(title, 100);
    // title is guaranteed to be 100 characters or less
}
```

#### ThrowIfLengthLessThan

Validates that a string meets a minimum length requirement.

```csharp
public void SetPassword(string password)
{
    ArgumentException.ThrowIfLengthLessThan(password, 8);
    // password is guaranteed to be at least 8 characters
}
```

#### ThrowIfLengthOutOfRange

Validates that a string length falls within a specified range.

```csharp
public void SetPostalCode(string postalCode)
{
    ArgumentException.ThrowIfLengthOutOfRange(postalCode, 5, 10);
    // postalCode length is between 5 and 10 characters
}
```

#### ThrowIfContainsWhiteSpace

Validates that a string does not contain any whitespace characters.

```csharp
public void SetIdentifier(string identifier)
{
    ArgumentException.ThrowIfContainsWhiteSpace(identifier);
    // identifier is guaranteed to have no spaces, tabs, or other whitespace
}
```

### Collection Validation

#### ThrowIfNullOrEmpty (Collections)

Validates that a collection is neither null nor empty. Supports multiple collection types.

```csharp
// IEnumerable<T>
public void ProcessItems(IEnumerable<string> items)
{
    ArgumentException.ThrowIfNullOrEmpty(items);
    // items is guaranteed to have at least one element
}

// ICollection<T>
public void ProcessList(ICollection<int> numbers)
{
    ArgumentException.ThrowIfNullOrEmpty(numbers);
}

// IReadOnlyCollection<T>
public void ProcessReadOnly(IReadOnlyCollection<string> values)
{
    ArgumentException.ThrowIfNullOrEmpty(values);
}

// Arrays
public void ProcessArray(string[] items)
{
    ArgumentException.ThrowIfNullOrEmpty(items);
}
```

#### ThrowIfCountGreaterThan

Validates that a collection does not exceed a maximum count.

```csharp
public void ProcessBatch(ICollection<Order> orders)
{
    ArgumentException.ThrowIfCountGreaterThan(orders, 1000);
    // orders collection has at most 1000 items
}
```

#### ThrowIfCountLessThan

Validates that a collection meets a minimum count requirement.

```csharp
public void ProcessTeam(IEnumerable<User> users)
{
    ArgumentException.ThrowIfCountLessThan(users, 2);
    // users collection has at least 2 members
}
```

#### ThrowIfCountOutOfRange

Validates that a collection count falls within a specified range.

```csharp
public void ProcessGroup(IReadOnlyCollection<Person> people)
{
    ArgumentException.ThrowIfCountOutOfRange(people, 3, 10);
    // people collection has between 3 and 10 members
}
```

#### ThrowIfContainsDuplicates

Validates that a collection does not contain duplicate elements.

```csharp
public void ProcessUniqueIds(IEnumerable<int> ids)
{
    ArgumentException.ThrowIfContainsDuplicates(ids);
    // ids collection contains only unique values
}

// With custom comparer
public void ProcessUniqueNames(IEnumerable<string> names)
{
    ArgumentException.ThrowIfContainsDuplicates(names, StringComparer.OrdinalIgnoreCase);
    // names collection contains unique values (case-insensitive)
}
```

### Numeric Range Validation

#### ThrowIfZero

Validates that a numeric value is not zero.

```csharp
public void SetDivisor(int divisor)
{
    ArgumentOutOfRangeException.ThrowIfZero(divisor);
    // divisor is guaranteed to be non-zero
}
```

#### ThrowIfNegative

Validates that a numeric value is not negative.

```csharp
public void SetQuantity(int quantity)
{
    ArgumentOutOfRangeException.ThrowIfNegative(quantity);
    // quantity is guaranteed to be >= 0
}

// Works with nint on older frameworks
public void SetNativeInt(nint value)
{
    ArgumentOutOfRangeException.ThrowIfNegative(value);
}
```

#### ThrowIfNegativeOrZero

Validates that a numeric value is positive (greater than zero).

```csharp
public void SetPrice(decimal price)
{
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);
    // price is guaranteed to be > 0
}
```

#### ThrowIfEqual

Validates that a value is not equal to a specified value.

```csharp
public void SetStatus(int status)
{
    ArgumentOutOfRangeException.ThrowIfEqual(status, 0);
    // status is guaranteed to be non-zero
}
```

#### ThrowIfNotEqual

Validates that a value equals a specified value.

```csharp
public void ProcessExpectedValue(int actual, int expected)
{
    ArgumentOutOfRangeException.ThrowIfNotEqual(actual, expected);
    // actual is guaranteed to equal expected
}
```

#### ThrowIfGreaterThan

Validates that a value does not exceed a maximum.

```csharp
public void SetPercentage(int percentage)
{
    ArgumentOutOfRangeException.ThrowIfGreaterThan(percentage, 100);
    // percentage is guaranteed to be <= 100
}
```

#### ThrowIfGreaterThanOrEqual

Validates that a value is strictly less than a maximum.

```csharp
public void SetIndex(int index, int arrayLength)
{
    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, arrayLength);
    // index is guaranteed to be < arrayLength (valid array index)
}
```

#### ThrowIfLessThan

Validates that a value meets a minimum requirement.

```csharp
public void SetAge(int age)
{
    ArgumentOutOfRangeException.ThrowIfLessThan(age, 18);
    // age is guaranteed to be >= 18
}
```

#### ThrowIfLessThanOrEqual

Validates that a value is strictly greater than a minimum.

```csharp
public void SetRating(int rating)
{
    ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(rating, 0);
    // rating is guaranteed to be > 0
}
```

#### ThrowIfOutOfRange

Validates that a value falls within a specified range (inclusive).

```csharp
public void SetVolume(int volume)
{
    ArgumentOutOfRangeException.ThrowIfOutOfRange(volume, 0, 100);
    // volume is guaranteed to be between 0 and 100 (inclusive)
}
```

### Date and Time Validation

These methods are available on .NET 8.0+ only, providing validation for temporal values.

#### ThrowIfInPast (DateTimeOffset)

Validates that a `DateTimeOffset` is not in the past.

```csharp
public void ScheduleEvent(DateTimeOffset eventTime)
{
    ArgumentOutOfRangeException.ThrowIfInPast(eventTime);
    // eventTime is guaranteed to be >= current UTC time
}

// With custom TimeProvider for testing
public void ScheduleEvent(DateTimeOffset eventTime, TimeProvider timeProvider)
{
    ArgumentOutOfRangeException.ThrowIfInPast(eventTime, timeProvider);
}
```

#### ThrowIfInFuture (DateTimeOffset)

Validates that a `DateTimeOffset` is not in the future.

```csharp
public void RecordTransaction(DateTimeOffset timestamp)
{
    ArgumentOutOfRangeException.ThrowIfInFuture(timestamp);
    // timestamp is guaranteed to be <= current UTC time
}
```

#### ThrowIfInPast (DateTime)

Validates that a `DateTime` is not in the past.

```csharp
public void ScheduleAppointment(DateTime appointmentTime)
{
    ArgumentOutOfRangeException.ThrowIfInPast(appointmentTime);
    // appointmentTime is guaranteed to be >= current UTC time
}
```

#### ThrowIfInFuture (DateTime)

Validates that a `DateTime` is not in the future.

```csharp
public void LogActivity(DateTime activityTime)
{
    ArgumentOutOfRangeException.ThrowIfInFuture(activityTime);
    // activityTime is guaranteed to be <= current UTC time
}
```

#### ThrowIfInPast (DateOnly)

Validates that a `DateOnly` is not in the past.

```csharp
public void BookEvent(DateOnly eventDate)
{
    ArgumentOutOfRangeException.ThrowIfInPast(eventDate);
    // eventDate is guaranteed to be >= today's date
}
```

#### ThrowIfInFuture (DateOnly)

Validates that a `DateOnly` is not in the future.

```csharp
public void RecordBirthdate(DateOnly birthdate)
{
    ArgumentOutOfRangeException.ThrowIfInFuture(birthdate);
    // birthdate is guaranteed to be <= today's date
}
```

### Special Type Validation

#### ThrowIfDefault

Validates that a value type is not its default value.

```csharp
public void ProcessId(Guid id)
{
    ArgumentException.ThrowIfDefault(id);
    // id is guaranteed to not be default(Guid)
}
```

#### ThrowIfEmptyGuid

Validates that a GUID is not `Guid.Empty`.

```csharp
public void SetUserId(Guid userId)
{
    ArgumentException.ThrowIfEmptyGuid(userId);
    // userId is guaranteed to not be Guid.Empty
}
```

### Backward Compatible Argument Class (Obsolete)

For projects migrating from older codebases, the `Argument` class provides backward-compatible helper methods. These are marked as obsolete and delegate to the modern exception polyfills.

```csharp
using NetEvolve.Arguments;

public void OldCodePattern(string value, int count)
{
    // Obsolete but functional - redirects to ArgumentException.ThrowIfNullOrEmpty
    Argument.ThrowIfNullOrEmpty(value);

    // Obsolete but functional - redirects to ArgumentOutOfRangeException.ThrowIfNegative
    Argument.ThrowIfNegative(count);
}
```

> [!NOTE]
> **Migration Recommendation**: Use the modern `ArgumentNullException`, `ArgumentException`, and `ArgumentOutOfRangeException` polyfill methods directly instead of the `Argument` class helpers.

## Cross-Framework Examples

### Example 1: .NET Framework 4.8 Project

```csharp
// File: UserValidator.cs
// Target Framework: net48

using System;

public class UserValidator
{
    public void ValidateUser(string email, int age, string[] roles)
    {
        // Modern .NET 8+ API works perfectly on .NET Framework 4.8!
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentOutOfRangeException.ThrowIfLessThan(age, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(age, 150);
        ArgumentException.ThrowIfNullOrEmpty(roles);
    }
}
```

### Example 2: Multi-Targeting Library

```csharp
// File: SharedLibrary.csproj
// <TargetFrameworks>netstandard2.0;net6.0;net8.0</TargetFrameworks>

using System;
using System.Collections.Generic;

namespace SharedLibrary
{
    public class DataProcessor
    {
        // Same code works across all target frameworks
        public void Process(IEnumerable<string> items, int batchSize)
        {
            ArgumentException.ThrowIfNullOrEmpty(items);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(batchSize, 1000);

            // Process items...
        }
    }
}
```

### Example 3: Modern .NET 10 with DateTimeOffset Validation

```csharp
// File: EventScheduler.cs
// Target Framework: net10.0

using System;

public class EventScheduler
{
    private readonly TimeProvider _timeProvider;

    public EventScheduler(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public void ScheduleEvent(string title, DateTimeOffset startTime, DateTimeOffset endTime)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfLengthGreaterThan(title, 200);

        // Date/time validation (available on .NET 8+)
        ArgumentOutOfRangeException.ThrowIfInPast(startTime, _timeProvider);
        ArgumentOutOfRangeException.ThrowIfInPast(endTime, _timeProvider);

        // Ensure end is after start
        if (endTime <= startTime)
        {
            throw new ArgumentException("End time must be after start time.", nameof(endTime));
        }
    }
}
```

## Performance Considerations

NetEvolve.Arguments is designed with performance in mind:

- **Aggressive Inlining**: Frequently-used validation methods use `[MethodImpl(MethodImplOptions.AggressiveInlining)]` for minimal call overhead
- **Stack Trace Optimization**: Methods are marked with `[StackTraceHidden]` to produce cleaner exception stack traces
- **Zero Allocation on Success**: Validation methods only allocate when throwing exceptions
- **Framework Delegation**: On .NET 8+, methods delegate directly to framework implementations for optimal performance
- **Smart Collection Checks**: Uses `TryGetNonEnumeratedCount` to avoid enumerating collections when possible

### Benchmark Example

```csharp
// On .NET 8+, this has virtually zero overhead
ArgumentException.ThrowIfNullOrWhiteSpace(username);

// Equivalent to calling the built-in .NET 8 method directly
// No additional layers, no wrapper overhead
```

## Requirements

### Supported Frameworks

- **.NET 10.0**
- **.NET 9.0**
- **.NET 8.0**
- **.NET 7.0**
- **.NET 6.0**
- **.NET Standard 2.0** (enables support for .NET Framework 4.7.2+, Xamarin, Unity, and more)
- **.NET Framework 4.7.2** (Windows only)
- **.NET Framework 4.8** (Windows only)
- **.NET Framework 4.8.1** (Windows only)

### Feature Availability by Framework

| Feature Category                   | .NET Framework 4.7.2-4.8.1 | .NET Standard 2.0 | .NET 6.0-7.0 | .NET 8.0+             |
| ---------------------------------- | -------------------------- | ----------------- | ------------ | --------------------- |
| Null Validation                    | ✅                         | ✅                | ✅           | ✅ (Framework Native) |
| String Validation                  | ✅                         | ✅                | ✅           | ✅ (Framework Native) |
| Collection Validation              | ✅                         | ✅                | ✅           | ✅                    |
| Numeric Range Validation           | ✅                         | ✅                | ✅           | ✅ (Framework Native) |
| DateTime/DateTimeOffset Validation | ❌                         | ❌                | ❌           | ✅                    |
| DateOnly Validation                | ❌                         | ❌                | ❌           | ✅                    |
| Extended Collection Helpers        | ✅                         | ✅                | ✅           | ✅                    |
| Special Type Validation            | ✅                         | ✅                | ✅           | ✅                    |

## Edge Cases and Special Scenarios

### Working with Nullable Value Types

```csharp
public void ProcessNullableInt(int? value)
{
    // ThrowIfNull works with nullable value types
    ArgumentNullException.ThrowIfNull(value);

    // After validation, value is guaranteed non-null
    int actualValue = value.Value; // Safe - no NullReferenceException
}
```

### Generic Type Constraints

```csharp
public class Repository<T> where T : IComparable<T>
{
    public void SetMinValue(T minValue, T maxValue)
    {
        // Generic constraints ensure compile-time safety
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minValue, maxValue);
    }
}
```

### Collection Performance Notes

```csharp
public void OptimizedCollectionCheck(IEnumerable<string> items)
{
    // Uses TryGetNonEnumeratedCount when possible
    // Avoids full enumeration for ICollection<T>, arrays, etc.
    ArgumentException.ThrowIfNullOrEmpty(items);

    // Only enumerates if count cannot be determined without enumeration
}
```

### Thread Safety

All validation methods are stateless and thread-safe. They can be safely called from multiple threads concurrently.

```csharp
public void ThreadSafeValidation(string input)
{
    // Safe to call from multiple threads
    ArgumentException.ThrowIfNullOrWhiteSpace(input);
}
```

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
