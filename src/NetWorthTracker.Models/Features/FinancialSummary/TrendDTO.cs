namespace NetWorthTracker.Core.Features.FinancialSummary;

public record TrendDTO(
    DateOnly SelectedDate,
    decimal CumulativeNetWorth,
    decimal AbsoluteNetWorthChange,
    float NetWorthPercentChange)
{
    public static TrendDTO Default => new(DateOnly.MinValue, 0, 0, 0);
}
