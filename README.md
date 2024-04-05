# NetEvolve.Arguments
Provides a set of backward compatible argument `throw` helper methods added in the latest .NET versions.
Especially intended for projects with multiple `TargetFrameworks`, for usage, standardization and maintainability.

## Method Overview
The following methods are currently provided.

### `Argument.ThrowIfEqual<T>(T, T, string)`
Throws an `ArgumentOutOfRangeException` if the first argument is equal to the second argument. Inplace replacement for [`ArgumentOutOfRangeException.ThrowIfEqual<T>(T, T, string)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifequal), which was introduced with **.NET 8**.

### `Argument.ThrowIfGreaterThan<T>(T, T, string)`
Throws an `ArgumentOutOfRangeException` if the first argument is greater than the second argument. Inplace replacement for [`ArgumentOutOfRangeException.ThrowIfGreaterThan<T>(T, T, string)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifgreaterthan), which was introduced with **.NET 8**.

### `Argument.ThrowIfGreaterThanOrEqual<T>(T, T, string)`
Throws an `ArgumentOutOfRangeException` if the first argument is greater than or equal to the second argument. Inplace replacement for [`ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual<T>(T, T, string)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifgreaterthanorequal), which was introduced with **.NET 8**.

### `Argument.ThrowIfLessThan<T>(T, T, string)`
Throws an `ArgumentOutOfRangeException` if the first argument is less than the second argument. Inplace replacement for [`ArgumentOutOfRangeException.ThrowIfLessThan<T>(T, T, string)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwiflessthan), which was introduced with **.NET 8**.

### `Argument.ThrowIfLessThanOrEqual<T>(T, T, string)`
Throws an `ArgumentOutOfRangeException` if the first argument is less than or equal to the second argument. Inplace replacement for [`ArgumentOutOfRangeException.ThrowIfLessThanOrEqual<T>(T, T, string)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwiflessthanorequal), which was introduced with **.NET 8**.

### `Argument.ThrowIfNotEqual<T>(T, T, string)`
Throws an `ArgumentOutOfRangeException` if the first argument is not equal to the second argument. Inplace replacement for [`ArgumentOutOfRangeException.ThrowIfNotEqual<T>(T, T, string)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifnotequal), which was introduced with **.NET 8**.

### `Argument.ThrowIfNull(object?, string)`
Throws an `ArgumentNullException` if the argument is `null`. Inplace replacement for [`ArgumentNullException.ThrowIfNull(object, string)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception.throwifnull), which was introduced with **.NET 6**.

### `Argument.ThrowIfNull(void*, string)`
Throws an `ArgumentNullException` if the argument is `null`. Inplace replacement for [`ArgumentNullException.ThrowIfNull(void*, string)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception.throwifnull?view=net-8.0#system-argumentnullexception-throwifnull(system-void*-system-string), which was introduced with **.NET 7**.

### `Argument.ThrowIfNullOrEmpty(string?, string)`
Throws an `ArgumentNullException` if the argument is `null` or throws an `ArgumentException` if the argument is empty. Inplace replacement for [`ArgumentException.ThrowIfNullOrEmpty(string, string)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception.throwifnullorempty), which was introduced with **.NET 7**.

### `Argument.ThrowIfNullOrWhiteSpace(string?, string)`
Throws an `ArgumentNullException` if the argument is `null` or throws an `ArgumentException` if the argument is empty or contains only white-space characters. Inplace replacement for [`ArgumentException.ThrowIfNullOrWhiteSpace(string, string)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception.throwifnullorwhitespace), which was introduced with **.NET 8**.