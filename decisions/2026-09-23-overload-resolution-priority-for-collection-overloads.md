---
authors:
  - Martin Stühmer

applyTo:
  - "src/NetEvolve.Arguments/**/*.cs"

created: 2026-09-23

lastModified: 2026-09-23

state: accepted

instructions: |
  Every public collection overload of a ThrowIf* method MUST carry an explicit [OverloadResolutionPriority] from most to least specific:
  T[] = 3, ICollection<T> = 2, IReadOnlyCollection<T> = 1, IEnumerable<T> = 0, non-generic IEnumerable = -1.
  New collection overloads MUST follow this ranking so that concrete collection types (List<T>, HashSet<T>, Dictionary<TKey, TValue>) never produce CS0121.
---

# Decision: Overload Resolution Priority for Collection Overloads

The collection overloads of the `ArgumentException.ThrowIf*` polyfills are ranked explicitly with `[OverloadResolutionPriority]` so that the C# compiler always picks exactly one overload.

## Context

`ThrowIfNullOrEmpty`, `ThrowIfCountGreaterThan`, `ThrowIfCountLessThan` and `ThrowIfCountOutOfRange` provide overloads for `IEnumerable<T>`, `ICollection<T>`, `IReadOnlyCollection<T>` and `T[]`. Most BCL collections (`List<T>`, `HashSet<T>`, `Dictionary<TKey, TValue>`, `ReadOnlyCollection<T>`) implement both `ICollection<T>` and `IReadOnlyCollection<T>`. Neither overload is better than the other, so these calls failed with `CS0121` (see issue #823). Callers had to cast, which also changed the `paramName` captured by `CallerArgumentExpression`.

## Decision

Every collection overload carries an explicit priority. Higher values win:

| Parameter type           | Priority |
| ------------------------ | -------- |
| `T[]`                    | 3        |
| `ICollection<T>`         | 2        |
| `IReadOnlyCollection<T>` | 1        |
| `IEnumerable<T>`         | 0        |
| `IEnumerable`            | -1       |

The non-generic `IEnumerable` overload is the fallback for types such as `ArrayList`.

`OverloadResolutionPriorityAttribute` is only part of the BCL from .NET 9. For older target frameworks an internal polyfill lives in `src/NetEvolve.Arguments/Polyfills/OverloadResolutionPriorityAttribute.cs`. The compiler recognizes the attribute by name when it reads the library's metadata, so consumers on every target framework get the same binding.

## Consequences

- Concrete collection types bind to the `ICollection<T>` overload without casting, and `paramName` is captured correctly.
- Arrays keep binding to the `T[]` overload; interfaces typed as `IReadOnlyCollection<T>` or `IEnumerable<T>` keep binding to their own overloads.
- Existing call sites stay source compatible; no signature changed.
- Consumers using a compiler older than C# 13 ignore the attribute and still see the ambiguity for concrete types, which matches the previous behavior.
- Every new collection overload must be assigned a priority from this table.

## Alternatives Considered

- **Priority only on `ICollection<T>`**: resolves the ambiguity but routes arrays away from the `T[]` overload, because priority is evaluated before betterness.
- **Additional, more specific overloads (e.g. `List<T>`)**: would have to be repeated for every concrete collection type and would still leave user-defined types ambiguous.

## Related Decisions (Optional)

- [.NET 10 and C# 13 Adoption](./2025-07-11-dotnet-10-csharp-13-adoption.md) - `OverloadResolutionPriority` requires C# 13 or later.
