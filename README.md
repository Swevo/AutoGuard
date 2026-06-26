# Swevo.AutoGuard

Compile-time guard clauses for .NET.

Apply `[AutoGuard]` to a partial class with a primary constructor, then decorate constructor parameters with guard attributes:

```csharp
using AutoGuard;

[AutoGuard]
public partial class CreateOrderCommand(
    [NotNull] string id,
    [NotEmpty] string customer,
    [InRange(1, 100)] int quantity,
    [Matches(@"^[A-Z0-9]+$")] string sku)
{
}
```

The source generator emits a private field initializer that validates arguments during construction and throws the expected framework exceptions.

## Guard attributes

- `[NotNull]`
- `[NotEmpty]` for `string` and `IEnumerable<T>`
- `[InRange(min, max)]`
- `[Matches(pattern)]`

## Package

- Package ID: `Swevo.AutoGuard`
- Target: Roslyn source generator / analyzer package
- Assembly packed to `analyzers/dotnet/cs`
