using Microsoft.EntityFrameworkCore;

using NetWorthTracker.Core.Features.Account;
using NetWorthTracker.Data.Api.Features.Account;

namespace NetWorthTracker.Data.Features.Account;

public class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _db;

    public AccountRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<AccountDTO>> GetAllAsync()
    {
        return await _db.Accounts.Select(categoryEntity => categoryEntity.ToModel()).ToListAsync();
    }

    public async Task<AccountDTO?> GetByIdAsync(int id)
    {
        var entity = await _db.Accounts.FindAsync(id);

        return entity?.ToModel();
    }

    public async Task AddAsync(AccountDTO account)
    {
        _db.Accounts.Add(account.ToEntity());

        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(AccountDTO account)
    {
        _db.Accounts.Update(account.ToEntity());

        await _db.SaveChangesAsync();
    }
}
