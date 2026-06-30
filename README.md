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


## Also by the same author

> 🌐 Full suite overview: **[swevo.github.io](https://swevo.github.io/)**

| Package | Description |
|---|---|
| [**AutoLog.Generator**](https://github.com/Swevo/AutoLog.Generator) | Compile-time high-performance logging — `[Log(Level, Message)]` generates `LoggerMessage.Define`. AOT-safe. |
| [**AutoHttpClient.Generator**](https://github.com/Swevo/AutoHttpClient.Generator) | Compile-time typed HTTP client — `[HttpClient]` on an interface generates a strongly-typed client. AOT-safe Refit alternative. |
| [**AutoDispatch.Generator**](https://github.com/Swevo/AutoDispatch.Generator) | Compile-time CQRS dispatcher — `[Handler]` generates a strongly-typed `IDispatcher`. MediatR alternative. |
| [**AutoWire**](https://github.com/Swevo/AutoWire) | Compile-time DI auto-registration for `Microsoft.Extensions.DependencyInjection`. |
| [**AutoMap.Generator**](https://github.com/Swevo/AutoMap.Generator) | Compile-time object mapping with generated extension methods. AutoMapper alternative. |

## License

MIT
