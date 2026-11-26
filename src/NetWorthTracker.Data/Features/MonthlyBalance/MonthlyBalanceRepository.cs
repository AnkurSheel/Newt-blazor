using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using NetWorthTracker.Business.Features.MonthlyBalances.Api;
using NetWorthTracker.Business.Features.MonthlyBalances.Persistence;

namespace NetWorthTracker.Data.Features.MonthlyBalance;

public class MonthlyBalanceRepository : IMonthlyBalanceRepository
{
    private const int SqliteUniqueConstraintErrorCode = 2067;
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
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException
                                           {
                                               SqliteExtendedErrorCode: SqliteUniqueConstraintErrorCode
                                           })
        {
            throw new DuplicateMonthlyBalanceException(ex);
        }
    }

    public async Task<decimal?> GetBalanceByDateAsync(int accountId, DateOnly monthDate)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        return await dbContext.MonthlyBalances.AsNoTracking()
            .Where(mb => mb.AccountId == accountId && mb.MonthDate == monthDate)
            .Select(mb => (decimal?)mb.Amount)
            .FirstOrDefaultAsync();
    }
}
