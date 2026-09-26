using Microsoft.EntityFrameworkCore;

using NetWorthTracker.Core.Features.MonthlyBalance;
using NetWorthTracker.Data.Api.Features.MonthlyBalance;

namespace NetWorthTracker.Data.Features.MonthlyBalance;

public class MonthlyBalanceRepository : IMonthlyBalanceRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public MonthlyBalanceRepository(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task AddAsync(MonthlyBalanceCreateDTO monthlyBalance)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        dbContext.MonthlyBalances.Add(
            new MonthlyBalanceEntity(monthlyBalance.AccountId, monthlyBalance.BalanceDate, monthlyBalance.Amount));
        await dbContext.SaveChangesAsync();
    }
}
