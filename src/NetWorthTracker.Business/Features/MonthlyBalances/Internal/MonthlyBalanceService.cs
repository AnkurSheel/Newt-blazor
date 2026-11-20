using NetWorthTracker.Business.Features.MonthlyBalances.Api;
using NetWorthTracker.Business.Features.MonthlyBalances.Persistence;

namespace NetWorthTracker.Business.Features.MonthlyBalances.Internal;

internal sealed class MonthlyBalanceService : IMonthlyBalanceService
{
    private readonly IMonthlyBalanceRepository _repository;

    public MonthlyBalanceService(IMonthlyBalanceRepository repository)
    {
        _repository = repository;
    }

    public async Task AddMonthlyBalanceAsync(MonthlyBalanceCreateDTO monthlyBalance)
    {
        await _repository.AddAsync(monthlyBalance);
    }

    public async Task<decimal?> GetBalanceByDateAsync(int accountId, DateOnly monthDate)
    {
        return await _repository.GetBalanceByDateAsync(accountId, monthDate);
    }
}
