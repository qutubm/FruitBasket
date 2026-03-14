using FruitBasket.Models;

namespace FruitBasket.PricingStrategy
{
    public class PerItemPricingStrategy : IPricingStrategy
    {
        public int DiscountQty = 10;
        public int DiscountPercentage = 10;

        public bool CanHandle(PricingContext context) =>
        context.PricingMethod == PricingMethodEnum.PerItem && 
            context.OrderDate.DayOfWeek != DayOfWeek.Saturday && 
            context.OrderDate.DayOfWeek != DayOfWeek.Sunday;

        public decimal CalculatePrice(PricingContext context)
        {
            var gross = context.Price * context.Qty;

            if(context.Qty > DiscountQty)
                return gross - (gross * DiscountPercentage / 100);

            return gross;
        }
    }
}
