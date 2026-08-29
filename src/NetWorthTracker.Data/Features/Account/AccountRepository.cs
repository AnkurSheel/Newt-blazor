#region
using Microsoft.EntityFrameworkCore;

using NetWorthTracker.Core.Features.Account;
using NetWorthTracker.Data.Api.Features.Account;
#endregion

namespace NetWorthTracker.Data.Features.Account;

public class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _db;

    public AccountRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<AccountResponseDTO>> GetAllAsync()
    {
        return await _db.Accounts.AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(accountEntity => new AccountResponseDTO
            {
                Name = accountEntity.Name,
                Type = accountEntity.Type,
                OpenDate = accountEntity.OpenDate,
                ClosedDate = accountEntity.ClosedDate,
                // EF Core translates this subquery into SQL
                LatestBalance = _db.MonthlyBalances.Where(monthlyBalanceEntity
                        => monthlyBalanceEntity.AccountId == accountEntity.Id)
                    .OrderByDescending(monthlyBalanceEntity => monthlyBalanceEntity.MonthDate)
                    .Select(monthlyBalanceEntity => monthlyBalanceEntity.Amount)
                    .FirstOrDefault() // Use synchronous FirstOrDefault inside the LINQ expression
            })
            .ToListAsync();
    }

    public async Task AddAsync(AccountCreateDTO account)
    {
        _db.Accounts.Add(new AccountEntity(account.Name, account.Type, account.OpenDate, account.ClosedDate));
        await _db.SaveChangesAsync();
    }
}
