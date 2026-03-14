using FruitBasket.Models;
using FruitBasket.PricingStrategy;

namespace FruitBasket.Tests;

public class PerItemPricingStrategyTests
{
    private readonly PerItemPricingStrategy _strategy = new();

    // CanHandle

    [Fact]
    public void CanHandle_PerItemOnWeekday_ReturnsTrue()
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
        Assert.True(result);
    }

    [Theory]
    [InlineData(DayOfWeek.Saturday)]
    [InlineData(DayOfWeek.Sunday)]
    public void CanHandle_PerItemOnWeekend_ReturnsFalse(DayOfWeek dayOfWeek)
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
        Assert.False(result);
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

    // CalculatePrice

    [Fact]
    public void CalculatePrice_BelowDiscountThreshold_ReturnsFullPrice()
    {
        // Arrange
        var context = new PricingContext
        {
            PricingMethod = PricingMethodEnum.PerItem,
            Price = 0.30M,
            Qty = 10, // at threshold, not above — no discount
            OrderDate = FakeTimeProvider.Weekday.DateTime
        };

        // Act
        var result = _strategy.CalculatePrice(context);

        // Assert
        Assert.Equal(3M, result); // 0.30 * 10 = 3
    }

    [Fact]
    public void CalculatePrice_AboveDiscountThreshold_AppliesDiscount()
    {
        // Arrange
        var context = new PricingContext
        {
            PricingMethod = PricingMethodEnum.PerItem,
            Price = 0.30M,
            Qty = 15,
            OrderDate = FakeTimeProvider.Weekday.DateTime
        };
        var gross = 0.30M * 15; // 4.50
        var expected = gross - (gross * _strategy.DiscountPercentage / 100);

        // Act
        var result = _strategy.CalculatePrice(context);

        // Assert
        Assert.Equal(expected, result);
    }
}
