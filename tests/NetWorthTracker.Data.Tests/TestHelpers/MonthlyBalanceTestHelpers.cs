using NetWorthTracker.Data.Features.MonthlyBalance;

namespace NetWorthTracker.Data.Tests.TestHelpers;

public static class MonthlyBalanceTestHelpers
{
    public static async Task AddMonthlyBalance(SqliteTestDatabase database, MonthlyBalanceEntity monthlyBalanceEntity)
    {
        await AddMonthlyBalances(database, new List<MonthlyBalanceEntity> { monthlyBalanceEntity });
    }

    public static async Task AddMonthlyBalances(
        SqliteTestDatabase database,
        IReadOnlyList<MonthlyBalanceEntity> monthlyBalanceEntities
    )
    {
        await using var dbContext = await database.CreateDbContextAsync();
        dbContext.MonthlyBalances.AddRange(monthlyBalanceEntities);
        await dbContext.SaveChangesAsync();
    }
}
