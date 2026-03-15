namespace FruitBasket.PricingStrategy;

public class SeasonalDiscountDecorator : IPricingStrategy
{
    private readonly IPricingStrategy _inner;
    private readonly int _discountPercentage;
    private readonly (int Month, int Day) _seasonStart;
    private readonly (int Month, int Day) _seasonEnd;

    public SeasonalDiscountDecorator(
        IPricingStrategy inner,
        int discountPercentage,
        (int Month, int Day) seasonStart,
        (int Month, int Day) seasonEnd)
    {
        _inner = inner;
        _discountPercentage = discountPercentage;
        _seasonStart = seasonStart;
        _seasonEnd = seasonEnd;
    }

    public bool CanHandle(PricingContext context) =>
        _inner.CanHandle(context) && IsInSeason(context.OrderDate);

    public decimal CalculatePrice(PricingContext context)
    {
        var price = _inner.CalculatePrice(context);
        return price - (price * _discountPercentage / 100);
    }

    private bool IsInSeason(DateTime date)
    {
        var start = new DateTime(date.Year, _seasonStart.Month, _seasonStart.Day);
        var end   = new DateTime(date.Year, _seasonEnd.Month,   _seasonEnd.Day);
        return date >= start && date <= end;
    }
}
