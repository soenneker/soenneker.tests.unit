[![](https://img.shields.io/nuget/v/Soenneker.Tests.Unit.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Tests.Unit/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.tests.unit/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.tests.unit/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Tests.Unit.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Tests.Unit/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.tests.unit/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.tests.unit/actions/workflows/codeql.yml)

# Soenneker.Tests.Unit

A TUnit base class with Bogus, AutoFaker, and per-test Serilog output, without a dependency-injection container.

## Installation

```bash
dotnet add package Soenneker.Tests.Unit
```

## Usage

```csharp
using Soenneker.Tests.Unit;

public sealed class OrderNumberTests : UnitTest
{
    [Test]
    public async Task Formats_customer_and_sequence()
    {
        string customer = Faker.Random.AlphaNumeric(8);
        var request = AutoFaker.Generate<CreateOrderRequest>();

        Logger.LogInformation("Checking order number for {Customer}", customer);

        string result = OrderNumber.Create(customer, request.Sequence);

        await Assert.That(result).StartsWith(customer);
    }
}
```

`Faker` and `AutoFaker` are created lazily for each test-class instance. The Bogus `Faker` exposed by the base is the one owned by that `AutoFaker`, so both follow the same generator configuration.

`Logger` writes through a private Serilog pipeline to the active TUnit test context. TUnit calls the base class's asynchronous lifecycle methods and the pipeline is released during test teardown.

## Choosing the right base

`UnitTest` intentionally has no `IServiceProvider` and cannot resolve application services. Use `Soenneker.Tests.HostedUnit` with `Soenneker.TestHosts.Unit` when the subject needs dependency injection or scoped services.

Derived infrastructure can pass an existing `AutoFaker` to the protected constructor. It can also set `enableLogging: false` when logging is supplied by a host, but must then configure `LazyLogger` before using `Logger` or a logged `Delay`.
