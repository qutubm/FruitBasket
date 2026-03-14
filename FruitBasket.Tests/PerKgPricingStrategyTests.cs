using FruitBasket.Models;
using FruitBasket.PricingStrategy;

namespace FruitBasket.Tests;

public class PerKgPricingStrategyTests
{
    private readonly PerKgPricingStrategy _strategy = new();

    // CanHandle

    [Fact]
    public void CanHandle_PerKgOnWeekday_ReturnsTrue()
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
        Assert.True(result);
    }

    [Theory]
    [InlineData(DayOfWeek.Saturday)]
    [InlineData(DayOfWeek.Sunday)]
    public void CanHandle_PerKgOnWeekend_ReturnsFalse(DayOfWeek dayOfWeek)
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
        Assert.False(result);
    }

    [Fact]
    public void CanHandle_PerItemOnWeekday_ReturnsFalse()
    {
        // Arrange
        var context = new PricingContext
        {
            PricingMethod = PricingMethodEnum.PerItem,
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
            PricingMethod = PricingMethodEnum.PerKg,
            Price = 2M,
            Qty = 2, // at threshold, not above — no discount
            OrderDate = FakeTimeProvider.Weekday.DateTime
        };

        // Act
        var result = _strategy.CalculatePrice(context);

        // Assert
        Assert.Equal(4M, result); // 2 * 2 = 4
    }

    [Fact]
    public void CalculatePrice_AboveDiscountThreshold_AppliesDiscount()
    {
        // Arrange
        var context = new PricingContext
        {
            PricingMethod = PricingMethodEnum.PerKg,
            Price = 2M,
            Qty = 5,
            OrderDate = FakeTimeProvider.Weekday.DateTime
        };
        var gross = 2M * 5; // 10
        var expected = gross - (gross * _strategy.DiscountPercentage / 100);

        // Act
        var result = _strategy.CalculatePrice(context);

        // Assert
        Assert.Equal(expected, result);
    }
}
