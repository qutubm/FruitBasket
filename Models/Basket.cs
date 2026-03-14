using FruitBasket.PricingStrategy;

namespace FruitBasket.Models;

public class Basket
{
    private IList<Fruit> _basket;
    private decimal _totalBill = 0;
    private readonly TimeProvider _timeProvider;
    private readonly IEnumerable<IPricingStrategy> _strategies;

    public Basket(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;

        _strategies = new IPricingStrategy[]
        {
            new PerKgWeekendPricingStrategy(), 
            new PerItemWeekendPricingStrategy(),
            new PerKgPricingStrategy(),
            new PerItemPricingStrategy()
        };

        _basket = new List<Fruit>();
    }

    public void Add(Fruit fruit)
    {
        if (fruit.PricingModel == PricingMethodEnum.PerItem && fruit.Qty % 1 != 0)
            throw new Exception("Partial quantity not allowed for Fruits measured as Per Item");

        var pricingContext = new PricingContext
        {
            PricingMethod = fruit.PricingModel,
            OrderDate = _timeProvider.GetLocalNow().DateTime,
            Price = fruit.BasePrice,
            Qty = fruit.Qty
        };

        var strategy = _strategies.FirstOrDefault(x => x.CanHandle(pricingContext));
        fruit.Amount = strategy?.CalculatePrice(pricingContext) ?? 0;
        _totalBill += fruit.Amount;
        _basket.Add(fruit);
    }

    public IEnumerable<Fruit> Fruits 
    { 
        get { return _basket; }
    }

    public decimal TotalBill 
    { 
        get { return _totalBill; } 
    }
}
