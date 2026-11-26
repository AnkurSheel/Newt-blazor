using NetWorthTracker.Business.Results;

namespace NetWorthTracker.Business.Features.MonthlyBalances.Api;

public interface IMonthlyBalanceService
{
    Task<Result> AddMonthlyBalanceAsync(MonthlyBalanceCreateDTO monthlyBalanceCreateDto);

    Task<decimal?> GetBalanceByDateAsync(int accountId, DateOnly monthDate);
}
