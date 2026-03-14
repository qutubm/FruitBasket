using FruitBasket.Models;

namespace FruitBasket.Tests;

public class BasketTests
{
    // TotalBill

    [Fact]
    public void TotalBill_EmptyBasket_ReturnsZero()
    {
        // Arrange
        var basket = new Basket(new FakeTimeProvider(FakeTimeProvider.Weekday));

        // Act
        var total = basket.TotalBill;

        // Assert
        Assert.Equal(0M, total);
    }

    [Fact]
    public void TotalBill_SinglePerKgFruit_ReturnsCorrectAmount()
    {
        // Arrange
        var basket = new Basket(new FakeTimeProvider(FakeTimeProvider.Weekday));

        // Act
        basket.Add(new Fruit { Name = "Apple", BasePrice = 2M, Qty = 1, PricingModel = PricingMethodEnum.PerKg });

        // Assert
        Assert.Equal(2M, basket.TotalBill); // 2 * 1 = 2, no discount
    }

    [Fact]
    public void TotalBill_SinglePerItemFruit_ReturnsCorrectAmount()
    {
        // Arrange
        var basket = new Basket(new FakeTimeProvider(FakeTimeProvider.Weekday));

        // Act
        basket.Add(new Fruit { Name = "Banana", BasePrice = 0.30M, Qty = 5, PricingModel = PricingMethodEnum.PerItem });

        // Assert
        Assert.Equal(1.50M, basket.TotalBill); // 0.30 * 5 = 1.50, no discount
    }

    [Fact]
    public void TotalBill_MultipleFruits_ReturnsSumOfAllAmounts()
    {
        // Arrange
        var basket = new Basket(new FakeTimeProvider(FakeTimeProvider.Weekday));

        // Act
        basket.Add(new Fruit { Name = "Apple", BasePrice = 2M, Qty = 1, PricingModel = PricingMethodEnum.PerKg });
        basket.Add(new Fruit { Name = "Banana", BasePrice = 0.30M, Qty = 5, PricingModel = PricingMethodEnum.PerItem });

        // Assert
        Assert.Equal(3.50M, basket.TotalBill); // 2.00 + 1.50
    }

    // Weekday pricing

    [Fact]
    public void TotalBill_PerKgOnWeekday_AboveDiscountThreshold_AppliesWeekdayDiscount()
    {
        // Arrange — PerKgPricingStrategy: DiscountQty=2, DiscountPercentage=7
        var basket = new Basket(new FakeTimeProvider(FakeTimeProvider.Weekday));
        var price = 2M;
        var qty = 5;
        var gross = price * qty; // 10
        var expected = gross - (gross * 7 / 100); // 9.30

        // Act
        basket.Add(new Fruit { Name = "Apple", BasePrice = price, Qty = qty, PricingModel = PricingMethodEnum.PerKg });

        // Assert
        Assert.Equal(expected, basket.TotalBill);
    }

    [Fact]
    public void TotalBill_PerItemOnWeekday_AboveDiscountThreshold_AppliesWeekdayDiscount()
    {
        // Arrange — PerItemPricingStrategy: DiscountQty=10, DiscountPercentage=10
        var basket = new Basket(new FakeTimeProvider(FakeTimeProvider.Weekday));
        var price = 0.30M;
        var qty = 15;
        var gross = price * qty; // 4.50
        var expected = gross - (gross * 10 / 100); // 4.05

        // Act
        basket.Add(new Fruit { Name = "Banana", BasePrice = price, Qty = qty, PricingModel = PricingMethodEnum.PerItem });

        // Assert
        Assert.Equal(expected, basket.TotalBill);
    }

    // Weekend pricing

    [Fact]
    public void TotalBill_PerKgOnSaturday_AboveDiscountThreshold_AppliesWeekendDiscount()
    {
        // Arrange — PerKgWeekendPricingStrategy: DiscountQty=2, DiscountPercentage=10
        var basket = new Basket(new FakeTimeProvider(FakeTimeProvider.Saturday));
        var price = 2M;
        var qty = 3;
        var gross = price * qty; // 6
        var expected = gross - (gross * 10 / 100); // 5.40

        // Act
        basket.Add(new Fruit { Name = "Apple", BasePrice = price, Qty = qty, PricingModel = PricingMethodEnum.PerKg });

        // Assert
        Assert.Equal(expected, basket.TotalBill);
    }

    [Fact]
    public void TotalBill_PerItemOnSunday_AboveDiscountThreshold_AppliesWeekendDiscount()
    {
        // Arrange — PerItemWeekendPricingStrategy: DiscountQty=8, DiscountPercentage=15
        var basket = new Basket(new FakeTimeProvider(FakeTimeProvider.Sunday));
        var price = 0.30M;
        var qty = 10;
        var gross = price * qty; // 3.00
        var expected = gross - (gross * 15 / 100); // 2.55

        // Act
        basket.Add(new Fruit { Name = "Banana", BasePrice = price, Qty = qty, PricingModel = PricingMethodEnum.PerItem });

        // Assert
        Assert.Equal(expected, basket.TotalBill);
    }

    // Fruit.Amount is set on Add

    [Fact]
    public void Add_SetsFruitAmountCorrectly()
    {
        // Arrange
        var basket = new Basket(new FakeTimeProvider(FakeTimeProvider.Weekday));
        var fruit = new Fruit { Name = "Apple", BasePrice = 2M, Qty = 1, PricingModel = PricingMethodEnum.PerKg };

        // Act
        basket.Add(fruit);

        // Assert
        Assert.Equal(2M, fruit.Amount);
    }
}
