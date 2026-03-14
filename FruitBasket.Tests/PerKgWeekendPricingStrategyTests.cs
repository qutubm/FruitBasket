using FruitBasket.Models;
using FruitBasket.PricingStrategy;

namespace FruitBasket.Tests;

public class PerKgWeekendPricingStrategyTests
{
    private readonly PerKgWeekendPricingStrategy _strategy = new();

    // CanHandle

    [Theory]
    [InlineData(DayOfWeek.Saturday)]
    [InlineData(DayOfWeek.Sunday)]
    public void CanHandle_PerKgOnWeekend_ReturnsTrue(DayOfWeek dayOfWeek)
    {
        // Arrange
        var date = dayOfWeek == DayOfWeek.Saturday ? FakeTimeProvider.Saturday : FakeTimeProvider.Sunday;
        var context = new PricingContext
        {
            PricingMethod = PricingMethodEnum.PerKg,
            Price = 2M,
            Qty = 1,
            OrderDate = date.DateTime
        };

        // Act
        var result = _strategy.CanHandle(context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanHandle_PerKgOnWeekday_ReturnsFalse()
    {
        // Arrange
        var context = new PricingContext
        {
            PricingMethod = PricingMethodEnum.PerKg,
            Price = 2M,
            Qty = 1,
            OrderDate = FakeTimeProvider.Weekday.DateTime
        };

        // Act
        var result = _strategy.CanHandle(context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanHandle_PerItemOnWeekend_ReturnsFalse()
    {
        // Arrange
        var context = new PricingContext
        {
            PricingMethod = PricingMethodEnum.PerItem,
            Price = 0.30M,
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
            PricingMethod = PricingMethodEnum.PerKg,
            Price = 5M,
            Qty = 1, // below threshold of 2 — no discount
            OrderDate = FakeTimeProvider.Saturday.DateTime
        };

        // Act
        var result = _strategy.CalculatePrice(context);

        // Assert
        Assert.Equal(5M, result);
    }

    [Fact]
    public void CalculatePrice_AtOrAboveDiscountThreshold_AppliesDiscount()
    {
        // Arrange
        var context = new PricingContext
        {
            PricingMethod = PricingMethodEnum.PerKg,
            Price = 5M,
            Qty = 3,
            OrderDate = FakeTimeProvider.Saturday.DateTime
        };
        var gross = 5M * 3; // 15
        var expected = gross - (gross * _strategy.DiscountPercentage / 100);

        // Act
        var result = _strategy.CalculatePrice(context);

        // Assert
        Assert.Equal(expected, result);
    }
}
