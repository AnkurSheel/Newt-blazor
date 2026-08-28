using NetWorthTracker.Core.Exceptions;
using NetWorthTracker.Core.Features.Account;
using NetWorthTracker.Data.Api.Features.Account;
using NetWorthTracker.Services.Api.Features.Account;

namespace NetWorthTracker.Services.Features.Account;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _repository;

    public AccountService(IAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AccountDTO>> GetAccountsAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task AddAccountAsync(string name, AccountType type)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("Category name is required.");
        }
        await _repository.AddAsync(new AccountDTO(0, name.Trim(), type, null));
    }

    public async Task UpdateAccountAsync(AccountDTO account)
    {
        if (string.IsNullOrWhiteSpace(account.Name))
        {
            throw new ValidationException("Category name is required.");
        }
        await _repository.UpdateAsync(account);
    }

    public Task CloseAccountAsync(AccountDTO account)
    {
        return _repository.UpdateAsync(account with { ClosedDate = DateTime.UtcNow });
    }
}
