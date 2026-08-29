[![](https://img.shields.io/nuget/v/Soenneker.Tests.Unit.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Tests.Unit/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.tests.unit/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.tests.unit/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Tests.Unit.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Tests.Unit/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.tests.unit/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.tests.unit/actions/workflows/codeql.yml)

# Soenneker.Tests.Unit

A base class providing Faker, AutoFaker, and logging. Does NOT have the ability to resolve services (there's no ServiceProvider involved when instantiating this).

## Install

```bash
dotnet add package Soenneker.Tests.Unit
```

## What you get

- `UnitTest` — A base class providing Faker, AutoFaker, and logging. Does NOT have the ability to resolve services (there's no ServiceProvider involved when instantiating this).

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `UnitTest.Faker` | Syntactic sugar for lazy Faker. | Syntactic sugar for lazy Faker. |
| `UnitTest.AutoFaker` | Used for generating fake objects with real values (without mocking). | Used for generating fake objects with real values (without mocking). |

## Practical notes

- Dispose instances you own when their scope ends so held resources can be released.
