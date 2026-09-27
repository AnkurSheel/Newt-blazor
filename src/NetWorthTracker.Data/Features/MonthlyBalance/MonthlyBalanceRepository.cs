using Microsoft.EntityFrameworkCore;

using NetWorthTracker.Core.Features.Account;
using NetWorthTracker.Core.Features.FinancialSummary;
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

    public async Task<MonthlySummaryDTO> GetMonthlySummaryAsync(DateOnly selectedDate)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        return await dbContext.MonthlyBalances.AsNoTracking()
                   .Where(mb => mb.MonthDate == selectedDate
                                && mb.Account.OpenDate <= mb.MonthDate
                                && (mb.Account.ClosedDate == null || mb.Account.ClosedDate > mb.MonthDate))
                   .Select(mb => new
                   {
                       mb.MonthDate,
                       mb.Amount,
                       AccountType = mb.Account.Type
                   })
                   .GroupBy(x => x.MonthDate)
                   .Select(g => new MonthlySummaryDTO(
                       g.Sum(x => x.AccountType == AccountType.ASSET ? x.Amount : 0m),
                       g.Sum(x => x.AccountType == AccountType.LIABILITY ? x.Amount : 0m)))
                   .FirstOrDefaultAsync()
               ?? new MonthlySummaryDTO(0, 0);
    }
}
