using NetWorthTracker.Core.Features.MonthlyBalance;
using NetWorthTracker.Data.Api.Features.MonthlyBalance;
using NetWorthTracker.Services.Api.Features.MonthlyBalance;

namespace NetWorthTracker.Services.Features.MonthlyBalance;

public class MonthlyBalanceService : IMonthlyBalanceService
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
}
