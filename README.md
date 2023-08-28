# NetEvolve.Arguments
Provides a set of backward compatible argument `throw` helper methods added in the latest .NET versions.
Especially intended for projects with multiple `TargetFrameworks`, for usage, standardization and maintainability.

## Method Overview
The following methods are currently provided.

### ThrowIfGreaterThan
Compatibility method to [`ArgumentOutOfRangeException.ThrowIfGreaterThan<T>(T, T, String)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifgreaterthan), which was introduced with .NET 8

### ThrowIfGreaterThanOrEqual
Compatibility method to [`ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual<T>(T, T, String)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwifgreaterthanorequal), which is part of the framework since .NET 8.

### ThrowIfLessThan
Compatibility method to [`ArgumentOutOfRangeException.ThrowIfLessThan<T>(T, T, String)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwiflessthan), which is part of the framework since .NET 8.

### ThrowIfLessThanOrEqual
Compatibility method to [`ArgumentOutOfRangeException.ThrowIfLessThanOrEqual<T>(T, T, String)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception.throwiflessthanorequal), which is part of the framework since .NET 8.

### ThrowIfNull
Compatibility method to [`ArgumentNullException.ThrowIfNull(Object, String)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception.throwifnull#system-argumentnullexception-throwifnull(system-object-system-string)), which is part of the framework since .NET 8.

### ThrowIfNullOrEmpty
Compatibility method to [`ArgumentException.ThrowIfNullOrEmpty(String, String)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception.throwifnullorempty), which is part of the framework since .NET 8.

### ThrowIfNullOrWhiteSpace
Compatibility method to [`ArgumentException.ThrowIfNullOrWhiteSpace(String, String)`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception.throwifnullorwhitespace), which is part of the framework since .NET 8.
