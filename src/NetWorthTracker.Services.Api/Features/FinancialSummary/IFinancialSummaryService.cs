using NetWorthTracker.Core.Features.FinancialSummary;

namespace NetWorthTracker.Services.Api.Features.FinancialSummary;

public interface IFinancialSummaryService
{
    Task<FinancialSummaryDTO> GetFinancialSummaryAsync(DateOnly selectedDate);
}
