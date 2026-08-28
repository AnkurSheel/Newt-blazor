using NetWorthTracker.Core.Features.Account;

namespace NetWorthTracker.Data.Api.Features.Account;

public interface IAccountRepository
{
    Task<IReadOnlyList<AccountDTO>> GetAllAsync();

    Task<AccountDTO?> GetByIdAsync(int id);

    Task AddAsync(AccountDTO account);

    Task UpdateAsync(AccountDTO account);
}
