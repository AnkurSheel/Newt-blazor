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

    public async Task<IReadOnlyList<AccountResponseDTO>> GetAllAsync(DateOnly selectedDate)
    {
        return await _db.Accounts.AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(accountEntity => new AccountResponseDTO
            {
                Id = accountEntity.Id,
                Name = accountEntity.Name,
                Type = accountEntity.Type,
                OpenDate = accountEntity.OpenDate,
                ClosedDate = accountEntity.ClosedDate,
                LatestBalance = _db.MonthlyBalances
                    .Where(mb => mb.AccountId == accountEntity.Id && mb.MonthDate <= selectedDate)
                    .OrderByDescending(mb => mb.MonthDate)
                    .Select(mb => mb.Amount)
                    .FirstOrDefault()
            })
            .ToListAsync();
    }

    public async Task AddAsync(AccountCreateDTO account)
    {
        _db.Accounts.Add(new AccountEntity(account.Name, account.Type, account.OpenDate, account.ClosedDate));
        await _db.SaveChangesAsync();
    }
}
