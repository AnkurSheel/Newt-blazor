using NetWorthTracker.Core.Features.MonthlyBalance;
using NetWorthTracker.Data.Api.Features.MonthlyBalance;

namespace NetWorthTracker.Data.Features.MonthlyBalance;

public class MonthlyBalanceRepository : IMonthlyBalanceRepository
{
    private readonly AppDbContext _dbContext;

    public MonthlyBalanceRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(MonthlyBalanceCreateDTO monthlyBalance)
    {
        _dbContext.MonthlyBalances.Add(
            new MonthlyBalanceEntity(monthlyBalance.AccountId, monthlyBalance.BalanceDate, monthlyBalance.Amount));
        await _dbContext.SaveChangesAsync();
    }
}
