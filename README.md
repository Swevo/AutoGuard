# Swevo.AutoGuard

[![NuGet](https://img.shields.io/nuget/v/Swevo.AutoGuard.svg)](https://www.nuget.org/packages/Swevo.AutoGuard)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Swevo.AutoGuard.svg)](https://www.nuget.org/packages/Swevo.AutoGuard)
[![CI](https://github.com/Swevo/AutoGuard/actions/workflows/build.yml/badge.svg)](https://github.com/Swevo/AutoGuard/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

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

## Related Packages

| Package | Downloads | Description |
|---|---|---|
| [Swevo.AutoBus](https://www.nuget.org/packages/Swevo.AutoBus) | [![Downloads](https://img.shields.io/nuget/dt/Swevo.AutoBus.svg)](https://www.nuget.org/packages/Swevo.AutoBus) | Free, MIT-licensed in-process message bus for  |
| [Swevo.AutoBus.RabbitMQ](https://www.nuget.org/packages/Swevo.AutoBus.RabbitMQ) | [![Downloads](https://img.shields.io/nuget/dt/Swevo.AutoBus.RabbitMQ.svg)](https://www.nuget.org/packages/Swevo.AutoBus.RabbitMQ) | RabbitMQ transport for AutoBus |
| [Swevo.AutoAssert](https://www.nuget.org/packages/Swevo.AutoAssert) | [![Downloads](https://img.shields.io/nuget/dt/Swevo.AutoAssert.svg)](https://www.nuget.org/packages/Swevo.AutoAssert) | Free, MIT-licensed fluent assertions for  |
| [Swevo.AutoAuth](https://www.nuget.org/packages/Swevo.AutoAuth) | [![Downloads](https://img.shields.io/nuget/dt/Swevo.AutoAuth.svg)](https://www.nuget.org/packages/Swevo.AutoAuth) | A free, MIT-licensed fluent configuration wrapper around OpenIddict for building OAuth2/OIDC token servers in ASP |
| [Swevo.AutoAudit](https://www.nuget.org/packages/Swevo.AutoAudit) | [![Downloads](https://img.shields.io/nuget/dt/Swevo.AutoAudit.svg)](https://www.nuget.org/packages/Swevo.AutoAudit) | Compile-time audit field generation for EF Core entities using Roslyn source generators |
| [Swevo.AutoResult](https://www.nuget.org/packages/Swevo.AutoResult) | [![Downloads](https://img.shields.io/nuget/dt/Swevo.AutoResult.svg)](https://www.nuget.org/packages/Swevo.AutoResult) | Compile-time Result<T> monad for  |
| [Swevo.AutoImage](https://www.nuget.org/packages/Swevo.AutoImage) | [![Downloads](https://img.shields.io/nuget/dt/Swevo.AutoImage.svg)](https://www.nuget.org/packages/Swevo.AutoImage) | A free, MIT-licensed fluent image processing wrapper around SkiaSharp for  |
| [Swevo.AutoFeatureFlag](https://www.nuget.org/packages/Swevo.AutoFeatureFlag) | [![Downloads](https://img.shields.io/nuget/dt/Swevo.AutoFeatureFlag.svg)](https://www.nuget.org/packages/Swevo.AutoFeatureFlag) | Compile-time feature flag stubs for  |
| [Swevo.AutoTestData](https://www.nuget.org/packages/Swevo.AutoTestData) | [![Downloads](https://img.shields.io/nuget/dt/Swevo.AutoTestData.svg)](https://www.nuget.org/packages/Swevo.AutoTestData) | Compile-time test data builders for  |

---

## License

MIT
