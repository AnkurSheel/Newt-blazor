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
                       g.Key,
                       g.Sum(x => x.AccountType == AccountType.ASSET ? x.Amount : 0m),
                       g.Sum(x => x.AccountType == AccountType.LIABILITY ? x.Amount : 0m)))
                   .FirstOrDefaultAsync()
               ?? MonthlySummaryDTO.Default;
    }

    public async Task<IReadOnlyList<MonthlySummaryDTO>> GetMonthlySummariesAsync(DateOnly selectedDate)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        return await dbContext.MonthlyBalances.AsNoTracking()
            .Where(mb => mb.MonthDate <= selectedDate
                         && mb.Account.OpenDate <= mb.MonthDate
                         && (mb.Account.ClosedDate == null || mb.Account.ClosedDate > mb.MonthDate))
            .Select(mb => new
            {
                mb.MonthDate,
                mb.Amount,
                AccountType = mb.Account.Type
            })
            .GroupBy(x => x.MonthDate)
            .OrderBy(g => g.Key)
            .Select(g => new MonthlySummaryDTO(
                g.Key,
                g.Sum(x => x.AccountType == AccountType.ASSET ? x.Amount : 0m),
                g.Sum(x => x.AccountType == AccountType.LIABILITY ? x.Amount : 0m)))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<MonthlySummaryDTO>> GetYearlySummariesAsync(DateOnly selectedDate)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        return await dbContext.MonthlyBalances.AsNoTracking()
            .Where(mb => mb.MonthDate <= selectedDate
                         && mb.Account.OpenDate <= mb.MonthDate
                         && (mb.Account.ClosedDate == null || mb.Account.ClosedDate > mb.MonthDate))
            .Where(mb => dbContext.MonthlyBalances.AsNoTracking()
                .Where(mb1 => mb1.MonthDate <= selectedDate
                              && mb1.Account.OpenDate <= mb1.MonthDate
                              && (mb1.Account.ClosedDate == null || mb1.Account.ClosedDate > mb1.MonthDate))
                .GroupBy(mb2 => mb2.MonthDate.Year)
                .Select(g => g.Max(mb3 => mb3.MonthDate))
                .Contains(mb.MonthDate))
            .Select(mb => new
            {
                mb.MonthDate,
                mb.Amount,
                AccountType = mb.Account.Type
            })
            .GroupBy(x => x.MonthDate)
            .OrderBy(g => g.Key)
            .Select(g => new MonthlySummaryDTO(
                g.Key,
                g.Sum(x => x.AccountType == AccountType.ASSET ? x.Amount : 0m),
                g.Sum(x => x.AccountType == AccountType.LIABILITY ? x.Amount : 0m)))
            .ToListAsync();
    }
}
