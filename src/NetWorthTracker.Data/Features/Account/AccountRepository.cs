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
            .Select(entity => entity.ToModel())
            .ToListAsync();
    }

    public async Task AddAsync(AccountCreateDTO account)
    {
        _db.Accounts.Add(account.ToEntity());
        await _db.SaveChangesAsync();
    }
}
