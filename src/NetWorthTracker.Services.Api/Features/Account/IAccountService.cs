using NetWorthTracker.Core.Features.Account;

namespace NetWorthTracker.Services.Api.Features.Account;

public interface IAccountService
{
    Task<IReadOnlyList<AccountDTO>> GetAccountsAsync();

    Task AddAccountAsync(string name, AccountType type);

    Task UpdateAccountAsync(AccountDTO account);

    Task CloseAccountAsync(AccountDTO account);
}
