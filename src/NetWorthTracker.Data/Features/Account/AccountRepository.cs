#region
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using NetWorthTracker.Business.Features.Accounts.Api;
using NetWorthTracker.Business.Features.Accounts.Persistence;
#endregion

namespace NetWorthTracker.Data.Features.Account;

public class AccountRepository : IAccountRepository
{
    private const int SqliteUniqueContraintErrorCode = 2067;
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
                BalanceOnDate = dbContext.MonthlyBalances.Where(mb
                        => mb.AccountId == accountEntity.Id && mb.MonthDate == selectedDate)
                    .Select(mb => (decimal?)mb.Amount)
                    .FirstOrDefault()
            })
            .ToListAsync();
    }

    public async Task AddAsync(AccountCreateDTO account)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        try
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            List<string> existingAccountNames = await dbContext.Accounts.Select(existingAccount => existingAccount.Name)
                .ToListAsync();
            if (existingAccountNames.Contains(account.Name, StringComparer.OrdinalIgnoreCase))
            {
                throw new DuplicateAccountException();
            }

            dbContext.Accounts.Add(new AccountEntity(account.Name, account.Type, account.OpenDate, account.ClosedDate));
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException
                                           {
                                               SqliteExtendedErrorCode: SqliteUniqueContraintErrorCode
                                           })
        {
            throw new DuplicateAccountException(ex);
        }
    }
}
