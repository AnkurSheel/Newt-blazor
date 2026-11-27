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

        return await GetAccountResponseQuery(dbContext, dbContext.Accounts.AsNoTracking(), selectedDate)
            .OrderBy(account => account.Id)
            .ToListAsync();
    }

    public async Task<AccountResponseDTO?> GetByIdAsync(int accountId, DateOnly selectedDate)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        return await GetAccountResponseQuery(
                dbContext,
                dbContext.Accounts.AsNoTracking().Where(account => account.Id == accountId),
                selectedDate)
            .FirstOrDefaultAsync();
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

    private static IQueryable<AccountResponseDTO> GetAccountResponseQuery(
        AppDbContext dbContext,
        IQueryable<AccountEntity> accounts,
        DateOnly selectedDate)
    {
        return accounts.Select(accountEntity => new AccountResponseDTO
        {
            Id = accountEntity.Id,
            Name = accountEntity.Name,
            Type = accountEntity.Type,
            OpenDate = accountEntity.OpenDate,
            ClosedDate = accountEntity.ClosedDate,
            BalanceOnDate = dbContext.MonthlyBalances.Where(monthlyBalance
                    => monthlyBalance.AccountId == accountEntity.Id && monthlyBalance.MonthDate == selectedDate)
                .Select(monthlyBalance => (decimal?)monthlyBalance.Amount)
                .FirstOrDefault()
        });
    }
}
