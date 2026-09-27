namespace NetWorthTracker.Core.Features.FinancialSummary;

public record MonthlySummaryDTO(decimal TotalAssets, decimal TotalLiabilities)
{
    public decimal NetWorth => TotalAssets - TotalLiabilities;
}
