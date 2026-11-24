using NetWorthTracker.Business.Features.FinancialSummary.Api;

namespace NetWorthTracker.Business.Features.FinancialSummary.Persistence;

public interface IFinancialSummaryRepository
{
    Task<MonthlySummaryDTO> GetMonthlySummaryAsync(DateOnly selectedDate);

    Task<IReadOnlyList<MonthlySummaryDTO>> GetMonthlySummariesAsync(DateOnly selectedDate);

    Task<IReadOnlyList<MonthlySummaryDTO>> GetYearlySummariesAsync(DateOnly selectedDate);
}
