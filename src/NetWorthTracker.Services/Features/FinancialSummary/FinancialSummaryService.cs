using NetWorthTracker.Core.Features.FinancialSummary;
using NetWorthTracker.Data.Api.Features.MonthlyBalance;
using NetWorthTracker.Services.Api.Features.FinancialSummary;

namespace NetWorthTracker.Services.Features.FinancialSummary;

public class FinancialSummaryService : IFinancialSummaryService
{
    private readonly IMonthlyBalanceRepository _monthlyBalanceRepository;

    public FinancialSummaryService(IMonthlyBalanceRepository monthlyBalanceRepository)
    {
        _monthlyBalanceRepository = monthlyBalanceRepository;
    }

    public async Task<MonthlySummaryDTO> GetMonthlySummary(DateOnly selectedDate)
    {
        return await _monthlyBalanceRepository.GetMonthlySummaryAsync(selectedDate);
    }
}
