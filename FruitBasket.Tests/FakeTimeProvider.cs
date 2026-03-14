namespace FruitBasket.Tests;

public class FakeTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _now;

    public FakeTimeProvider(DateTimeOffset now) => _now = now;

    public override DateTimeOffset GetUtcNow() => _now;

    // Known weekday/weekend dates for reuse across tests
    public static readonly DateTimeOffset Weekday = new DateTimeOffset(2024, 1, 15, 12, 0, 0, TimeSpan.Zero); // Monday
    public static readonly DateTimeOffset Saturday = new DateTimeOffset(2024, 1, 13, 12, 0, 0, TimeSpan.Zero);
    public static readonly DateTimeOffset Sunday  = new DateTimeOffset(2024, 1, 14, 12, 0, 0, TimeSpan.Zero);
}
