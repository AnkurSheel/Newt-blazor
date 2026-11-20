using NetWorthTracker.Business.Features.MonthlyBalances.Api;

namespace NetWorthTracker.Business.Features.MonthlyBalances.Persistence;

public interface IMonthlyBalanceRepository
{
    Task AddAsync(MonthlyBalanceCreateDTO monthlyBalance);

    Task<decimal?> GetBalanceByDateAsync(int accountId, DateOnly monthDate);
}
