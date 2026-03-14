using FruitBasket.Models;

namespace FruitBasket.PricingStrategy;

public class PricingContext
{
    public required PricingMethodEnum PricingMethod { get; init; }
    public required decimal Price { get; set; }
    public required decimal Qty { get; set; }
    public required DateTime OrderDate { get; set; }
}
