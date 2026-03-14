namespace FruitBasket.PricingStrategy;

public interface IPricingStrategy
{
    bool CanHandle(PricingContext context);
    decimal CalculatePrice(PricingContext context);
}
