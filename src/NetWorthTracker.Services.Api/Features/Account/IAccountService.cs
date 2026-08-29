using NetWorthTracker.Core.Features.Account;

namespace NetWorthTracker.Services.Api.Features.Account;

public interface IAccountService
{
    Task<IReadOnlyList<AccountResponseDTO>> GetAccountsAsync();

    Task AddAccountAsync(AccountCreateDTO account);
}
