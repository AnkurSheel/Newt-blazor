#region
using Microsoft.EntityFrameworkCore;

using NetWorthTracker.Core.Features.Account;
using NetWorthTracker.Data.Api.Features.Account;
#endregion

namespace NetWorthTracker.Data.Features.Account;

public class AccountRepository : IAccountRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public AccountRepository(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IReadOnlyList<AccountResponseDTO>> GetAllAsync(DateOnly selectedDate)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        return await dbContext.Accounts.AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(accountEntity => new AccountResponseDTO
            {
                Id = accountEntity.Id,
                Name = accountEntity.Name,
                Type = accountEntity.Type,
                OpenDate = accountEntity.OpenDate,
                ClosedDate = accountEntity.ClosedDate,
                LatestBalance = dbContext.MonthlyBalances
                    .Where(mb => mb.AccountId == accountEntity.Id && mb.MonthDate == selectedDate)
                    .OrderByDescending(mb => mb.MonthDate)
                    .Select(mb => mb.Amount)
                    .FirstOrDefault()
            })
            .ToListAsync();
    }

    public async Task AddAsync(AccountCreateDTO account)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        dbContext.Accounts.Add(new AccountEntity(account.Name, account.Type, account.OpenDate, account.ClosedDate));
        await dbContext.SaveChangesAsync();
    }
}
