using NetWorthTracker.Core.Features.FinancialSummary;
using NetWorthTracker.Core.Features.MonthlyBalance;

namespace NetWorthTracker.Data.Api.Features.MonthlyBalance;

public interface IMonthlyBalanceRepository
{
    Task AddAsync(MonthlyBalanceCreateDTO monthlyBalance);

    Task<MonthlySummaryDTO> GetMonthlySummaryAsync(DateOnly selectedDate);
}
