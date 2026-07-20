---
name: writing-xunit-tests
description: >
  Write, create, modernize, or fix xUnit unit tests using AutoFixture for test data generation.
  USE FOR: write, create, review, or modernize xUnit tests and assertions,
  xUnit assertion APIs (Equal, Same, Null, NotNull, Empty, NotEmpty, Single, Contains,
  DoesNotContain, IsType, IsAssignableFrom, Throws, ThrowsAsync, InRange, Matches),
  data-driven tests (InlineData, MemberData, ClassData),
  AutoFixture test data generation (Fixture, AutoData, InlineAutoData, MemberAutoData, Frozen,
  AutoMoqCustomization), test lifecycle (constructor/Dispose, IAsyncLifetime, IClassFixture,
  ICollectionFixture), parallelization (Collection, CollectionDefinition), xUnit analyzer
  (xUnitxxxx) fixes.
  DO NOT USE FOR: test quality audits (use test-anti-patterns), running tests (use run-tests),
  MSTest tests (use writing-mstest-tests), NUnit/TUnit, or non-.NET languages.
---

# Writing xUnit Tests

Help write effective xUnit unit tests that use AutoFixture for test data generation, for any
test project in this repo that adopts xUnit (the current test projects under `Tests/` use
MSTest — see `writing-mstest-tests` for those; this skill is for xUnit-based projects).

## When to Use

- User wants to write new xUnit unit tests
- User wants to improve or modernize existing xUnit tests
- User asks about xUnit assertion APIs, data-driven patterns, or test lifecycle
- User wants to generate test data with AutoFixture instead of hand-building objects
- User asks to auto-mock constructor dependencies with `AutoFixture.AutoMoq`
- User needs help fixing a specific xUnit test bug or failing assertion
- User asks to fix or understand an xUnit analyzer diagnostic (an `xUnitxxxx` warning/error)

## When Not to Use

- User needs a test quality audit, anti-pattern detection, or flaky-test investigation (use `test-anti-patterns`)
- User needs to run or execute tests (use the `run-tests` skill)
- User is testing code in one of the existing `Tests/Claims/*` or `Tests/Common/*` projects — those are MSTest (use `writing-mstest-tests`)
- User needs CI/CD pipeline configuration
- User is using MSTest, NUnit, or TUnit (not xUnit)

## Inputs

| Input | Required | Description |
|-------|----------|-------------|
| Code under test | No | The production code to be tested |
| Existing test code | No | Current tests to fix, update, or modernize |
| Test scenario description | No | What behavior the user wants to test |

## Response Guidelines

- **Specific API or pattern questions** (assertions, data-driven, lifecycle, AutoFixture): jump directly to the relevant workflow step. Do not follow the full workflow.
- **Write new tests from scratch**: follow the full workflow.
- **Review and fix existing tests**: fix only the issues present. Do not add unrelated improvements.

## Workflow

### Step 1: Determine project setup

New xUnit test project:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit.v3" Version="1.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.*" />
    <PackageReference Include="AutoFixture" Version="4.*" />
    <PackageReference Include="AutoFixture.Xunit3" Version="4.*" />
    <PackageReference Include="AutoFixture.AutoMoq" Version="4.*" />
    <PackageReference Include="Moq" Version="4.*" />
  </ItemGroup>
</Project>
```

- `xunit.v3` is the current major version (native Microsoft Testing Platform support, same execution model this repo's MSTest projects already get from `MSTest.Sdk`). If a project instead targets `xunit` v2, swap in `AutoFixture.Xunit2` for the AutoFixture attribute integration.
- `AutoFixture.AutoMoq` pairs `Fixture` with Moq (already used across this repo's MSTest projects) so constructor dependencies get auto-mocked instead of hand-wired.
- Pin exact versions the same way `global.json` pins `MSTest.Sdk` for the existing projects, so upgrades are deliberate.

### Step 2: Write test classes following conventions

- Use `[Fact]` for parameterless tests, `[Theory]` for parameterized tests — no class-level attribute is needed (unlike MSTest's `[TestClass]`)
- Follow the **Arrange-Act-Assert** (AAA) pattern
- Name tests using `MethodName_Scenario_ExpectedBehavior`
- Seal test classes with `sealed` for consistency with the rest of the codebase
- Use separate test projects with naming convention `[ProjectName].Tests`

```csharp
public sealed class OrderServiceTests
{
    [Fact]
    public void CalculateTotal_WithDiscount_ReturnsReducedPrice()
    {
        // Arrange
        var service = new OrderService();
        var order = new Order { Price = 100m, DiscountPercent = 10 };

        // Act
        var total = service.CalculateTotal(order);

        // Assert
        Assert.Equal(90m, total);
    }
}
```

### Step 3: Use the right assertion API

| What you are testing | xUnit assertion |
|---|---|
| Two values are equal | `Assert.Equal(expected, actual)` |
| Same object instance (reference identity) | `Assert.Same(expected, actual)` |
| Value is null | `Assert.Null(value)` |
| Value is not null | `Assert.NotNull(value)` |
| Collection is empty | `Assert.Empty(collection)` |
| Collection is not empty | `Assert.NotEmpty(collection)` |
| Collection has exactly one item | `Assert.Single(collection)` (returns the element) |
| Collection has exactly N items | `Assert.Equal(N, collection.Count())` — no dedicated `HasCount` |
| Collection contains an item | `Assert.Contains(item, collection)` |
| Collection does not contain an item | `Assert.DoesNotContain(item, collection)` |
| Object is exactly a specific type | `Assert.IsType<T>(value)` (returns the typed instance) |
| Object is a type or subtype (e.g. an interface) | `Assert.IsType<T>(value, exactMatch: false)` |
| Code throws an exception | `Assert.Throws<T>(() => ...)` / `await Assert.ThrowsAsync<T>(...)` |

Prefer these specific assertions over `Assert.True(...)` with a hand-rolled boolean condition — they produce far better failure messages.

#### Equality, null, and reference checks

```csharp
Assert.Equal(expected, actual);   // Value equality
Assert.Same(expected, actual);    // Reference equality -- same object instance
Assert.Null(value);
Assert.NotNull(value);
```

#### Exception testing

```csharp
// Synchronous
var ex = Assert.Throws<ArgumentNullException>(() => service.Process(null));
Assert.Equal("input", ex.ParamName);

// Async
var ex = await Assert.ThrowsAsync<InvalidOperationException>(
    async () => await service.ProcessAsync(null));
```

`Assert.Throws<T>` (and `ThrowsAsync<T>`) match the exact type `T`; xUnit has no separate "matches `T` or derived type" variant — use `Assert.ThrowsAny<T>` / `ThrowsAnyAsync<T>` when a derived exception type is acceptable.

#### Collection assertions

```csharp
Assert.Contains(expectedItem, collection);
Assert.DoesNotContain(unexpectedItem, collection);
var single = Assert.Single(collection);        // Returns the single element
Assert.Equal(3, collection.Count());            // No dedicated HasCount
Assert.Empty(collection);
Assert.NotEmpty(collection);
```

Replace generic `Assert.True` with specialized assertions:

| Instead of | Use |
|---|---|
| `Assert.True(list.Count > 0)` | `Assert.NotEmpty(list)` |
| `Assert.True(list.Count == 0)` | `Assert.Empty(list)` |
| `Assert.True(list.Count() == 3)` | `Assert.Equal(3, list.Count())` |
| `Assert.True(x != null)` | `Assert.NotNull(x)` |
| `Assert.True(x == null)` | `Assert.Null(x)` |
| `Assert.Equal(a, b)` for same instance | `Assert.Same(a, b)` -- reference identity |
| `Assert.True(!list.Contains(item))` | `Assert.DoesNotContain(item, list)` |
| `Assert.True(list.Contains(item))` | `Assert.Contains(item, list)` |

#### String assertions

```csharp
Assert.Contains("expected", actualString);
Assert.StartsWith("prefix", actualString);
Assert.EndsWith("suffix", actualString);
Assert.Matches(@"\d{3}-\d{4}", phoneNumber);
```

#### Type assertions

```csharp
var typed = Assert.IsType<MyHandler>(result);                       // Exact type, returns typed instance
var typed = Assert.IsType<MyHandler>(result, exactMatch: false);    // Type or subtype (e.g. an interface)
```

#### Comparison assertions

xUnit has no dedicated "greater than"/"less than" assert; use `Assert.True` with the comparison or `Assert.InRange`:

```csharp
Assert.True(actual > lowerBound);
Assert.True(actual < upperBound);
Assert.InRange(actual, low, high);
```

### Step 4: Use data-driven tests for multiple inputs

#### InlineData for literal values

```csharp
[Theory]
[InlineData(1, 2, 3)]
[InlineData(0, 0, 0)]
[InlineData(-1, 1, 0)]
public void Add_ReturnsExpectedSum(int a, int b, int expected)
{
    Assert.Equal(expected, Calculator.Add(a, b));
}
```

#### MemberData for computed or tuple data

```csharp
[Theory]
[MemberData(nameof(DiscountTestData))]
public void ApplyDiscount_ReturnsExpectedPrice(decimal price, int percent, decimal expected)
{
    var result = PriceCalculator.ApplyDiscount(price, percent);
    Assert.Equal(expected, result);
}

public static IEnumerable<object[]> DiscountTestData =>
[
    [100m, 10, 90m],
    [200m, 25, 150m],
    [50m, 0, 50m],
];
```

`TheoryData<T1, T2, T3>` gives the same type safety MSTest gets from `ValueTuple`-based `DynamicData`, and is preferred over raw `IEnumerable<object[]>`:

```csharp
public static TheoryData<decimal, int, decimal> DiscountTestData => new()
{
    { 100m, 10, 90m },
    { 200m, 25, 150m },
    { 50m, 0, 50m },
};
```

#### AutoFixture for generated data (preferred over hand-written InlineData when the values themselves don't matter)

When a test only cares that *some* valid value flows through, let AutoFixture generate it instead of inventing arbitrary literals:

```csharp
[Theory, AutoData]
public void CalculateTotal_AnyOrder_NeverReturnsNegative(Order order)
{
    var service = new OrderService();

    var total = service.CalculateTotal(order);

    Assert.True(total >= 0);
}
```

Mix explicit and generated values with `[InlineAutoData]` — literals fill the leading parameters, AutoFixture fills the rest:

```csharp
[Theory]
[InlineAutoData(0)]     // discountPercent = 0, order is auto-generated
[InlineAutoData(100)]   // discountPercent = 100, order is auto-generated
public void ApplyDiscount_AnyOrder_StaysWithinBounds(int discountPercent, Order order)
{
    var result = PriceCalculator.ApplyDiscount(order, discountPercent);
    Assert.InRange(result, 0, order.Price);
}
```

`[MemberAutoData(nameof(Source))]` combines a `MemberData` source with AutoFixture filling any parameters the source doesn't provide, the same way `[InlineAutoData]` does for `[InlineData]`.

### Step 5: Handle test lifecycle correctly

xUnit has no `[TestInitialize]`/`[TestCleanup]` attributes — lifecycle is expressed through the constructor and standard .NET disposal:

- **Constructor** runs before every test method (xUnit creates a new test class instance per test) — use it for sync setup
- **`IDisposable.Dispose()`** runs after every test method — use it for sync teardown
- **`IAsyncLifetime`** (`InitializeAsync`/`DisposeAsync`) — use for async setup/teardown instead of a blocking constructor or `Dispose`
- **`IClassFixture<T>`** — shared context created once per test class (constructor-inject `T`)
- **`ICollectionFixture<T>`** + `[Collection("name")]` — shared context across multiple test classes

```csharp
public sealed class RepositoryTests : IAsyncLifetime
{
    private readonly FakeDatabase _db = new();

    public async Task InitializeAsync() => await _db.SeedAsync();

    public Task DisposeAsync()
    {
        _db.Reset();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetById_ExistingRecord_ReturnsIt()
    {
        var result = await _db.GetByIdAsync(1);
        Assert.NotNull(result);
    }
}
```

There is no `TestContext` equivalent — for diagnostic output, constructor-inject `ITestOutputHelper`:

```csharp
public sealed class OrderServiceTests(ITestOutputHelper output)
{
    [Fact]
    public void CalculateTotal_LogsIntermediateValue()
    {
        output.WriteLine("running CalculateTotal test");
        // ...
    }
}
```

#### Execution order

1. Constructor -- once per test method (new instance every time)
2. `IAsyncLifetime.InitializeAsync` (if implemented)
3. Test method
4. `IAsyncLifetime.DisposeAsync` (if implemented) -> `IDisposable.Dispose` (if implemented)
5. `IClassFixture<T>` instance -- created once, shared across all tests in the class, disposed after the last one
6. `ICollectionFixture<T>` instance -- created once, shared across all classes in the `[Collection]`, disposed after the last one

### Step 6: Apply cancellation and timeout patterns

xUnit v2 has no built-in per-test `[Timeout]`; use a `CancellationTokenSource` explicitly:

```csharp
[Fact]
public async Task FetchData_ReturnsWithinTimeout()
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

    var result = await _client.GetDataAsync(cts.Token);

    Assert.NotNull(result);
}
```

On xUnit v3, prefer the ambient `TestContext.Current.CancellationToken`, which is cancelled automatically if the test run is aborted:

```csharp
[Fact]
public async Task FetchData_ReturnsWithinTimeout()
{
    var result = await _client.GetDataAsync(TestContext.Current.CancellationToken);
    Assert.NotNull(result);
}
```

### Step 7: Use advanced features where appropriate

#### Skipping tests

```csharp
[Fact(Skip = "Flaky against external service -- tracked in ISSUE-123")]
public void ExternalService_EventuallyResponds() { }
```

xUnit has no built-in `[Retry]`, `[OSCondition]`, or `[CICondition]`. For conditional execution, guard at the top of the test with `Skip.If`/`Skip.IfNot` (xUnit v3) or an early `return` with a documented reason; for OS/CI targeting specifically, check `OperatingSystem.IsWindows()` etc. or the relevant environment variable and skip accordingly. Use `[Trait("Category", "...")]` to categorize tests (e.g. `"Integration"`, `"Slow"`) for selective filtering via `run-tests`, rather than relying on conditional skip logic.

#### Parallelization

xUnit parallelizes across test **collections** by default (each test class is its own implicit collection unless grouped). To disable parallelization for a specific set of classes, put them in an explicit collection and disable it:

```csharp
[CollectionDefinition("Database", DisableParallelization = true)]
public sealed class DatabaseCollection : ICollectionFixture<DatabaseFixture>;

[Collection("Database")]
public sealed class OrderRepositoryTests
{
    // ...
}
```

To disable parallelization assembly-wide, add `xunit.runner.json` with `"parallelizeTestCollections": false`.

### Step 8: Fix xUnit analyzer diagnostics (xUnitxxxx)

The `xunit.analyzers` package (bundled with the `xunit`/`xunit.v3` package) reports `xUnitxxxx` diagnostics during build and in the IDE. Most have an automated code fix. When fixing by hand, apply the idiomatic change rather than suppressing the rule.

| Rule | Problem | Fix |
|---|---|---|
| xUnit2000 | `Assert.Equal` args swapped | Put `expected` first, `actual` second |
| xUnit2002 | `Assert.NotNull` used on a value type | Remove the assertion (value types can't be null) or use `Assert.True(x != default)` |
| xUnit2003 / xUnit2004 | `Assert.True`/`Assert.False` used for a null/bool comparison | Use `Assert.Null`/`Assert.NotNull`/`Assert.True(x)`/`Assert.False(x)` directly (Step 3) |
| xUnit2005 / xUnit2006 | `Assert.Same`/`Assert.Equal` used for a same-type string/reference comparison better served by a specific overload | Use the more specific assertion the analyzer suggests |
| xUnit2007 / xUnit2008 / xUnit2009 / xUnit2010 / xUnit2011 / xUnit2012 / xUnit2017 | Sub-optimal assert on collections/types | Use the specific assert (`Assert.IsType`, `Assert.Contains`, `Assert.Empty`, etc.) (Step 3) |
| xUnit2013 | `Assert.Equal(N, collection.Count())` where `N == 1` | Use `Assert.Single(collection)` |
| xUnit1012 | `null` passed directly as `[InlineData]` value without a cast | Cast the `null` to the parameter's type |
| xUnit1026 | Unused `[Theory]` parameter | Remove the parameter or use it in the assertion |
| xUnit1031 | Blocking call (`.Result`, `.Wait()`) in an async test | `await` the call instead |
| xUnit1051 | Test method missing `CancellationToken` propagation | Flow `TestContext.Current.CancellationToken` (or a local `CancellationTokenSource`) into the awaited call (Step 6) |
| xUnit3001 | Constructor parameter not resolvable by any fixture | Take the dependency via `IClassFixture<T>`/`ICollectionFixture<T>` or construct it directly, not as a raw constructor parameter |

## AutoFixture reference

### Fixture basics

```csharp
var fixture = new Fixture();

var order = fixture.Create<Order>();               // Single object with all properties populated
var orders = fixture.CreateMany<Order>(3);          // IEnumerable<Order> with 3 items
```

`Fixture` recursively fills every settable property/constructor parameter with generated values (strings become `"<PropertyName><guid-fragment>"`, numbers increment, etc.), so tests don't need to invent irrelevant values for fields the test doesn't care about.

### AutoMoqCustomization -- pairs AutoFixture with this repo's existing Moq usage

```csharp
var fixture = new Fixture().Customize(new AutoMoqCustomization());

// Constructor dependencies are automatically Moq mocks
var sut = fixture.Create<ClaimHandler>();

// Get the auto-generated mock for a dependency to set up/verify against it
var repositoryMock = fixture.Freeze<Mock<IClaimRepository>>();
repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(fixture.Create<Claim>());
```

### [Frozen] -- share one instance across the SUT and assertions

Without `[Frozen]`, each parameter of the same type gets a *different* generated instance. `[Frozen]` pins the first-generated instance of a type so every subsequent parameter/dependency of that type reuses it — essential for asserting against the same mock that was injected into the system under test:

```csharp
[Theory, AutoData]
public async Task GetClaim_ExistingId_ReturnsMappedResult(
    [Frozen] Mock<IClaimRepository> repositoryMock,
    Claim claim,
    ClaimHandler sut)
{
    repositoryMock.Setup(r => r.GetByIdAsync(claim.Id)).ReturnsAsync(claim);

    var result = await sut.GetByIdAsync(claim.Id);

    Assert.Equal(claim.Id, result.Value.Id);
    repositoryMock.Verify(r => r.GetByIdAsync(claim.Id), Times.Once);
}
```

`sut` here is auto-constructed by AutoFixture/AutoMoq, receiving the same frozen `repositoryMock.Object` that the test configured and later verifies against.

### When not to use AutoFixture

Prefer explicit, meaningful values instead of `fixture.Create<T>()` when:

- The value drives a validation rule or business invariant the test is specifically exercising (e.g. a booking number format, a date range boundary) — a random generated value obscures *why* the test passes or fails
- The test's assertions depend on a specific relationship between fields (e.g. `EndDate > StartDate`) that AutoFixture won't respect by default
- Readability suffers — if a reviewer can't tell what's being tested without stepping through fixture customizations, fall back to explicit construction

This mirrors how `Tests/Claims/Shared/ClaimTestDataFactory.cs` already mixes explicit parameters with Bogus-generated filler for the existing MSTest suites: explicit values for what the test cares about, generated values for everything else.
