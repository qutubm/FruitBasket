using FruitBasket.Models;

namespace FruitBasket.PricingStrategy;

public class PerItemWeekendPricingStrategy : IPricingStrategy
{
    public int DiscountQty = 8;
    public int DiscountPercentage = 15;
    public bool CanHandle(PricingContext context) =>
    context.PricingMethod == PricingMethodEnum.PerItem && 
        (context.OrderDate.DayOfWeek == DayOfWeek.Saturday || 
         context.OrderDate.DayOfWeek == DayOfWeek.Sunday);

    public decimal CalculatePrice(PricingContext context)
    {
        var gross = context.Price * context.Qty;

        if(context.Qty >= DiscountQty)
            return gross - (gross * DiscountPercentage / 100);

        return gross;
    }
}
