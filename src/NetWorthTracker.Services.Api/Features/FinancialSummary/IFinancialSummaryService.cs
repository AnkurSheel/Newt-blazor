using NetWorthTracker.Core.Features.FinancialSummary;

namespace NetWorthTracker.Services.Api.Features.FinancialSummary;

public interface IFinancialSummaryService
{
    Task<MonthlySummaryDTO> GetMonthlySummary(DateOnly selectedDate);

    Task<MonthlyNetworthChangeDTO> GetMonthlyNetworthChangeAsync(DateOnly selectedDate);
}
