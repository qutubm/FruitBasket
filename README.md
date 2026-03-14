# Fruit Basket — Sales Calculator

A small .NET 8 system for calculating the total price of fruit orders, supporting multiple pricing strategies including per-kg, per-item, and weekend pricing with discounts.

---

## Getting Started

### Prerequisites
- .NET 8 SDK or higher

### Run the application
```bash
dotnet run --project FruitBasket.csproj
```

### Run the tests
```bash
dotnet test FruitBasket.Tests/FruitBasket.Tests.csproj
```

---

## Project Structure

```
FruitBasket/
├── Models/
│   ├── Fruit.cs                          # Fruit entity (name, price, qty, pricing method)
│   ├── Basket.cs                         # Order container — adds fruits and accumulates total
│   └── PricingMethodEnum.cs              # Enum: PerKg | PerItem
├── PricingStrategy/
│   ├── IPricingStrategy.cs               # Strategy interface
│   ├── PricingContext.cs                 # Data passed into each strategy
│   ├── PerKgPricingStrategy.cs           # Per-kg weekday pricing (7% discount above 2kg)
│   ├── PerItemPricingStrategy.cs         # Per-item weekday pricing (10% discount above 10 items)
│   ├── PerKgWeekendPricingStrategy.cs    # Per-kg weekend pricing (10% discount at 2kg+)
│   └── PerItemWeekendPricingStrategy.cs  # Per-item weekend pricing (15% discount at 8+ items)
├── Program.cs                            # Entry point with sample order
└── FruitBasket.Tests/
    ├── FakeTimeProvider.cs               # Test helper to control order date
    ├── BasketTests.cs
    ├── PerKgPricingStrategyTests.cs
    ├── PerItemPricingStrategyTests.cs
    ├── PerKgWeekendPricingStrategyTests.cs
    └── PerItemWeekendPricingStrategyTests.cs
```

---

## Design Decisions

### Strategy Pattern

The core design pattern used is the **Strategy Pattern**. Each pricing model is encapsulated in its own class that implements `IPricingStrategy`:

```csharp
public interface IPricingStrategy
{
    bool CanHandle(PricingContext context);
    decimal CalculatePrice(PricingContext context);
}
```

`Basket` holds an ordered list of all strategies and uses `CanHandle` to select the right one at the point a fruit is added:

```csharp
var strategy = _strategies.FirstOrDefault(x => x.CanHandle(pricingContext));
fruit.Amount = strategy?.CalculatePrice(pricingContext) ?? 0;
```

This means:
- Each pricing rule is isolated and independently testable
- The `Basket` class never needs to change when a new pricing model is introduced
- Strategies are evaluated in priority order — weekend strategies are checked before their weekday counterparts, so the more specific rule always wins

### PricingContext

Rather than passing raw parameters, a `PricingContext` object is used as the input to both `CanHandle` and `CalculatePrice`. This keeps the interface stable — adding new data (e.g. a customer loyalty tier) only requires adding a property to `PricingContext`, not changing every strategy's signature.

### TimeProvider Injection

`Basket` accepts a `TimeProvider` (a built-in .NET 8 abstraction) via its constructor. This makes weekend/weekday behaviour fully testable without relying on the real system clock. In production, `TimeProvider.System` is passed in; in tests, a `FakeTimeProvider` supplies a fixed date.

---

## Design Patterns Used

| Pattern | Where | Why |
|---|---|---|
| **Strategy** | `IPricingStrategy` + all `*PricingStrategy` classes | Encapsulates each pricing algorithm behind a common interface, making it easy to add new strategies without modifying existing code (Open/Closed Principle) |

---

## Pricing Strategies

| Strategy | Applies when | Discount |
|---|---|---|
| `PerKgPricingStrategy` | PerKg, weekday | 7% when qty > 2 |
| `PerItemPricingStrategy` | PerItem, weekday | 10% when qty > 10 |
| `PerKgWeekendPricingStrategy` | PerKg, Saturday or Sunday | 10% when qty ≥ 2 |
| `PerItemWeekendPricingStrategy` | PerItem, Saturday or Sunday | 15% when qty ≥ 8 |

---

## How to Extend

### Add a new fruit type

No code changes needed — just instantiate a `Fruit` with the appropriate `PricingModel` and `BasePrice`:

```csharp
basket.Add(new Fruit { Name = "Cherry", BasePrice = 5.00M, Qty = 3, PricingModel = PricingMethodEnum.PerKg });
```

### Add a new pricing strategy (e.g. bulk discount, loyalty pricing)

1. Add a value to `PricingMethodEnum` if it represents a new method type:
    ```csharp
    public enum PricingMethodEnum { PerKg, PerItem, PerBundle }
    ```

2. Create a new class implementing `IPricingStrategy`:
    ```csharp
    public class PerBundlePricingStrategy : IPricingStrategy
    {
        public bool CanHandle(PricingContext context) =>
            context.PricingMethod == PricingMethodEnum.PerBundle;

        public decimal CalculatePrice(PricingContext context) =>
            Math.Floor(context.Qty / 3M) * context.Price * 2 + (context.Qty % 3) * context.Price;
    }
    ```

3. Register it in `Basket`'s strategy list:
    ```csharp
    _strategies = new IPricingStrategy[]
    {
        new PerBundlePricingStrategy(),
        new PerKgWeekendPricingStrategy(),
        // ...
    };
    ```

4. Write unit tests in `FruitBasket.Tests` following the same Arrange/Act/Assert pattern.

### Add a discount model (e.g. Cherry with 10% off above 2kg)

The existing `PerKgPricingStrategy` already supports configurable discount thresholds via public fields (`DiscountQty`, `DiscountPercentage`). For a fruit-specific discount strategy, create a dedicated strategy class (as above) and set `CanHandle` to match both the pricing method and a fruit name or a new enum value.

### Add contextual data (e.g. customer loyalty tier)

Add the new property to `PricingContext`:
```csharp
public bool IsLoyaltyMember { get; init; }
```

Strategies that need it can inspect `context.IsLoyaltyMember` in both `CanHandle` and `CalculatePrice`. Strategies that don't care about it are completely unaffected.

### Add a basket-level discount (e.g. spend $30, get 5% off the whole order)

Basket-level discounts apply after all line items are calculated, so they live in `Basket` rather than in any individual strategy. Add an `ApplyBasketDiscount` method that inspects `_totalBill` and reduces it accordingly:

```csharp
public void ApplyBasketDiscount(decimal thresholdAmount, int discountPercentage)
{
    if (_totalBill >= thresholdAmount)
        _totalBill -= _totalBill * discountPercentage / 100;
}
```

This keeps the boundary clear: strategies own per-fruit pricing, and `Basket` owns order-level adjustments.

### Add tiered/volume pricing (e.g. 1–5 items = $2.00, 6–10 = $1.80, 11+ = $1.60)

Create a new strategy that applies different unit prices based on quantity bands. Add a new enum value and implement `CalculatePrice` with a simple range check:

```csharp
public class TieredPricingStrategy : IPricingStrategy
{
    public bool CanHandle(PricingContext context) =>
        context.PricingMethod == PricingMethodEnum.Tiered;

    public decimal CalculatePrice(PricingContext context) => context.Qty switch
    {
        <= 5  => context.Price * context.Qty,
        <= 10 => context.Price * 0.90M * context.Qty,
        _     => context.Price * 0.80M * context.Qty
    };
}
```

No changes are needed to `Basket`, `IPricingStrategy`, or any existing strategy.

### Add promotional / voucher code support

Add a `VoucherCode` property to `PricingContext`:

```csharp
public string? VoucherCode { get; init; }
```

Then create a `VoucherPricingStrategy` (or handle it in `Basket.ApplyBasketDiscount`) that checks for a known code and applies the associated discount. Voucher validation logic (e.g. expiry, single-use) can live in a separate `IVoucherService` injected into `Basket`.

### Stack multiple discounts using the Decorator pattern

When two discounts must apply to the same fruit (e.g. a weekend discount *and* a loyalty discount), the Strategy pattern alone picks only one strategy per fruit. Wrapping strategies with a decorator allows them to chain:

```csharp
public class LoyaltyDiscountDecorator : IPricingStrategy
{
    private readonly IPricingStrategy _inner;
    private readonly int _loyaltyDiscountPercentage = 5;

    public LoyaltyDiscountDecorator(IPricingStrategy inner) => _inner = inner;

    public bool CanHandle(PricingContext context) =>
        context.IsLoyaltyMember && _inner.CanHandle(context);

    public decimal CalculatePrice(PricingContext context)
    {
        var price = _inner.CalculatePrice(context);
        return price - (price * _loyaltyDiscountPercentage / 100);
    }
}
```

Register it by wrapping an existing strategy:
```csharp
new LoyaltyDiscountDecorator(new PerKgWeekendPricingStrategy())
```

### Add a strategy factory

Instead of hardcoding the strategy list inside `Basket`, a factory centralises strategy creation and makes it easier to load strategies from configuration or swap them at runtime:

```csharp
public static class PricingStrategyFactory
{
    public static IEnumerable<IPricingStrategy> Create() =>
    [
        new PerKgWeekendPricingStrategy(),
        new PerItemWeekendPricingStrategy(),
        new PerKgPricingStrategy(),
        new PerItemPricingStrategy()
    ];
}
```

`Basket` then becomes:
```csharp
_strategies = PricingStrategyFactory.Create();
```

This also makes it straightforward to inject a mock strategy list in tests.

### Add tax calculation

Tax is a post-pricing concern and belongs in `Basket`, applied after the total is accumulated:

```csharp
public decimal TotalBillWithTax(decimal taxRatePercent) =>
    _totalBill + (_totalBill * taxRatePercent / 100);
```

Keeping tax separate from pricing strategies means tax rate changes never touch the discount logic.

---

## Test Coverage

Tests are written with XUnit using Arrange/Act/Assert notation.

```
Line coverage:    92%
Branch coverage:  93.3%
Method coverage:  92.3%
```

All pricing strategy classes and `Basket` reach **100% coverage** of business logic. The only uncovered code is `Program.cs` (the console entry point, not meaningful to unit test).

| Test class | What it covers |
|---|---|
| `PerKgPricingStrategyTests` | `CanHandle` routing, full-price and discounted calculation |
| `PerItemPricingStrategyTests` | `CanHandle` routing, full-price and discounted calculation |
| `PerKgWeekendPricingStrategyTests` | Weekend routing, Sat/Sun both covered, discount threshold |
| `PerItemWeekendPricingStrategyTests` | Weekend routing, Sat/Sun both covered, discount threshold |
| `BasketTests` | Empty basket, single/multiple fruits, weekday vs weekend totals, `fruit.Amount` set on add |
