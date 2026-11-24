namespace NetWorthTracker.Business.Features.MonthlyBalances.Api;

public interface IMonthlyBalanceService
{
    Task AddMonthlyBalanceAsync(MonthlyBalanceCreateDTO monthlyBalance);

    Task<decimal?> GetBalanceByDateAsync(int accountId, DateOnly monthDate);
}
