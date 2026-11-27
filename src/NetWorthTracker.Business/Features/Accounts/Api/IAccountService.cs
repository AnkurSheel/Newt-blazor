using NetWorthTracker.Business.Results;

namespace NetWorthTracker.Business.Features.Accounts.Api;

public interface IAccountService
{
    Task<IReadOnlyList<AccountResponseDTO>> GetAccountsAsync(DateOnly selectedDate);

    Task<IReadOnlyList<AccountResponseDTO>> GetActiveAccountsAsync(DateOnly selectedDate);

    Task<IReadOnlyList<AccountResponseDTO>> GetClosedAccountsAsync(DateOnly selectedDate);

    Task<Result> AddAccountAsync(AccountCreateDTO account);

    Task<bool> IsAccountActive(int accountId, DateOnly selectedDate);
}
