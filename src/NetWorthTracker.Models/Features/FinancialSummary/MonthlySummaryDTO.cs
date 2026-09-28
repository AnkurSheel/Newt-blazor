namespace NetWorthTracker.Core.Features.FinancialSummary;

public record MonthlySummaryDTO(DateOnly SelectedDate, decimal TotalAssets, decimal TotalLiabilities)
{
    public decimal NetWorth => TotalAssets - TotalLiabilities;

    public static MonthlySummaryDTO Default => new(DateOnly.MinValue, 0, 0);
}
