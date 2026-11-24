using NetWorthTracker.Business.Results;

namespace NetWorthTracker.Business.Features.Accounts.Api;

public interface IAccountService
{
    Task<IReadOnlyList<AccountResponseDTO>> GetAccountsAsync(DateOnly selectedDate);

    Task<Result> AddAccountAsync(AccountCreateDTO account);
}
