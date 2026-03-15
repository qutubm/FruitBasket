using FruitBasket.Models;
using FruitBasket.PricingStrategy;

namespace FruitBasket.Tests;

public class SeasonalDiscountDecoratorTests
{
    private static readonly (int Month, int Day) SeasonStart = (Month: 12, Day: 20);
    private static readonly (int Month, int Day) SeasonEnd   = (Month: 12, Day: 31);

    // Dec 25, 2024 = Wednesday (weekday), within season
    private static readonly DateTime InSeasonWeekday  = new DateTime(2024, 12, 25);
    // Dec 21, 2024 = Saturday, within season but a weekend
    private static readonly DateTime InSeasonWeekend  = new DateTime(2024, 12, 21);
    // Dec 20, 2024 = Friday (weekday), exactly on season start
    private static readonly DateTime OnSeasonStart    = new DateTime(2024, 12, 20);
    // Dec 31, 2024 = Tuesday (weekday), exactly on season end
    private static readonly DateTime OnSeasonEnd      = new DateTime(2024, 12, 31);
    // Dec 19, 2024 = Thursday (weekday), one day before season
    private static readonly DateTime DayBeforeSeason  = new DateTime(2024, 12, 19);
    // Jan 1, 2025 = Wednesday (weekday), one day after season
    private static readonly DateTime DayAfterSeason   = new DateTime(2025, 1, 1);

    private static PricingContext PerKgContext(DateTime date, decimal price = 2M, decimal qty = 1M) =>
        new PricingContext
        {
            PricingMethod = PricingMethodEnum.PerKg,
            Price = price,
            Qty = qty,
            OrderDate = date
        };

    // CanHandle

    [Fact]
    public void CanHandle_InSeasonWeekday_ReturnsTrue()
    {
        var strategy = new SeasonalDiscountDecorator(new PerKgPricingStrategy(), 15, SeasonStart, SeasonEnd);

        Assert.True(strategy.CanHandle(PerKgContext(InSeasonWeekday)));
    }

    [Fact]
    public void CanHandle_OutOfSeason_ReturnsFalse()
    {
        var strategy = new SeasonalDiscountDecorator(new PerKgPricingStrategy(), 15, SeasonStart, SeasonEnd);

        Assert.False(strategy.CanHandle(PerKgContext(FakeTimeProvider.Weekday.DateTime)));
    }

    [Fact]
    public void CanHandle_InSeason_InnerCannotHandle_ReturnsFalse()
    {
        // Dec 21 is a Saturday — PerKgPricingStrategy won't handle weekends
        var strategy = new SeasonalDiscountDecorator(new PerKgPricingStrategy(), 15, SeasonStart, SeasonEnd);

        Assert.False(strategy.CanHandle(PerKgContext(InSeasonWeekend)));
    }

    [Fact]
    public void CanHandle_OnSeasonStartDate_ReturnsTrue()
    {
        var strategy = new SeasonalDiscountDecorator(new PerKgPricingStrategy(), 15, SeasonStart, SeasonEnd);

        Assert.True(strategy.CanHandle(PerKgContext(OnSeasonStart)));
    }

    [Fact]
    public void CanHandle_OnSeasonEndDate_ReturnsTrue()
    {
        var strategy = new SeasonalDiscountDecorator(new PerKgPricingStrategy(), 15, SeasonStart, SeasonEnd);

        Assert.True(strategy.CanHandle(PerKgContext(OnSeasonEnd)));
    }

    [Fact]
    public void CanHandle_DayBeforeSeasonStart_ReturnsFalse()
    {
        var strategy = new SeasonalDiscountDecorator(new PerKgPricingStrategy(), 15, SeasonStart, SeasonEnd);

        Assert.False(strategy.CanHandle(PerKgContext(DayBeforeSeason)));
    }

    [Fact]
    public void CanHandle_DayAfterSeasonEnd_ReturnsFalse()
    {
        var strategy = new SeasonalDiscountDecorator(new PerKgPricingStrategy(), 15, SeasonStart, SeasonEnd);

        Assert.False(strategy.CanHandle(PerKgContext(DayAfterSeason)));
    }

    // CalculatePrice

    [Fact]
    public void CalculatePrice_AppliesSeasonalDiscountToInnerPrice()
    {
        // Arrange — qty=1, below PerKg discount threshold, so inner price = 2*1 = 2.00
        // Seasonal 15% applied: 2.00 - (2.00 * 15/100) = 1.70
        var discountPct = 15;
        var strategy = new SeasonalDiscountDecorator(new PerKgPricingStrategy(), discountPct, SeasonStart, SeasonEnd);
        var context = PerKgContext(InSeasonWeekday, price: 2M, qty: 1M);
        var innerPrice = 2M * 1M;
        var expected = innerPrice - (innerPrice * discountPct / 100); // 1.70

        Assert.Equal(expected, strategy.CalculatePrice(context));
    }

    [Fact]
    public void CalculatePrice_InnerDiscountAppliedFirst_ThenSeasonalDiscountOnTop()
    {
        // Arrange — qty=5, above PerKg discount threshold (>2), inner applies 7% first
        // Inner: 2*5=10.00, minus 7% = 9.30
        // Seasonal 15% on inner result: 9.30 - (9.30 * 15/100) = 7.905
        var discountPct = 15;
        var inner = new PerKgPricingStrategy();
        var strategy = new SeasonalDiscountDecorator(inner, discountPct, SeasonStart, SeasonEnd);
        var context = PerKgContext(InSeasonWeekday, price: 2M, qty: 5M);
        var gross = 2M * 5M;
        var innerPrice = gross - (gross * inner.DiscountPercentage / 100); // 9.30
        var expected = innerPrice - (innerPrice * discountPct / 100);      // 7.905

        Assert.Equal(expected, strategy.CalculatePrice(context));
    }
}
