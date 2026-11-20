namespace NetWorthTracker.Business.Features.Accounts.Api;

public interface IAccountService
{
    Task<IReadOnlyList<AccountResponseDTO>> GetAccountsAsync(DateOnly selectedDate);

    Task AddAccountAsync(AccountCreateDTO account);
}
