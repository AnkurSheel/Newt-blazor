namespace NetWorthTracker.Business.Features.FinancialSummary.Api;

public interface IFinancialSummaryService
{
    Task<MonthlySummaryDTO> GetMonthlySummary(DateOnly selectedDate);

    Task<MonthlyNetworthChangeDTO> GetMonthlyNetworthChangeAsync(DateOnly selectedDate);

    Task<IReadOnlyList<TrendDTO>> GetMonthlyTrendsAsync(DateOnly selectedDate);

    Task<IReadOnlyList<TrendDTO>> GetYearlyTrendsAsync(DateOnly selectedDate);
}
