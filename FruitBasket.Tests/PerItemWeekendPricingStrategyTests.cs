using FruitBasket.Models;
using FruitBasket.PricingStrategy;

namespace FruitBasket.Tests;

public class PerItemWeekendPricingStrategyTests
{
    private readonly PerItemWeekendPricingStrategy _strategy = new();

    // CanHandle

    [Theory]
    [InlineData(DayOfWeek.Saturday)]
    [InlineData(DayOfWeek.Sunday)]
    public void CanHandle_PerItemOnWeekend_ReturnsTrue(DayOfWeek dayOfWeek)
    {
        // Arrange
        var date = dayOfWeek == DayOfWeek.Saturday ? FakeTimeProvider.Saturday : FakeTimeProvider.Sunday;
        var context = new PricingContext
        {
            PricingMethod = PricingMethodEnum.PerItem,
            Price = 0.30M,
            Qty = 1,
            OrderDate = date.DateTime
        };

        // Act
        var result = _strategy.CanHandle(context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanHandle_PerItemOnWeekday_ReturnsFalse()
    {
        // Arrange
        var context = new PricingContext
        {
            PricingMethod = PricingMethodEnum.PerItem,
            Price = 0.30M,
            Qty = 1,
            OrderDate = FakeTimeProvider.Weekday.DateTime
        };

        // Act
        var result = _strategy.CanHandle(context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanHandle_PerKgOnWeekend_ReturnsFalse()
    {
        // Arrange
        var context = new PricingContext
        {
            PricingMethod = PricingMethodEnum.PerKg,
            Price = 2M,
            Qty = 1,
            OrderDate = FakeTimeProvider.Saturday.DateTime
        };

        // Act
        var result = _strategy.CanHandle(context);

        // Assert
        Assert.False(result);
    }

    // CalculatePrice

    [Fact]
    public void CalculatePrice_BelowDiscountThreshold_ReturnsFullPrice()
    {
        // Arrange
        var context = new PricingContext
        {
            PricingMethod = PricingMethodEnum.PerItem,
            Price = 0.30M,
            Qty = 5, // below threshold of 8 — no discount
            OrderDate = FakeTimeProvider.Saturday.DateTime
        };

        // Act
        var result = _strategy.CalculatePrice(context);

        // Assert
        Assert.Equal(1.50M, result); // 0.30 * 5
    }

    [Fact]
    public void CalculatePrice_AtOrAboveDiscountThreshold_AppliesDiscount()
    {
        // Arrange
        var context = new PricingContext
        {
            PricingMethod = PricingMethodEnum.PerItem,
            Price = 0.30M,
            Qty = 10,
            OrderDate = FakeTimeProvider.Saturday.DateTime
        };
        var gross = 0.30M * 10; // 3.00
        var expected = gross - (gross * _strategy.DiscountPercentage / 100);

        // Act
        var result = _strategy.CalculatePrice(context);

        // Assert
        Assert.Equal(expected, result);
    }
}
