using NetWorthTracker.Business.Features.Accounts.Api;
using NetWorthTracker.Business.Features.Accounts.Persistence;
using NetWorthTracker.Core.Exceptions;

namespace NetWorthTracker.Business.Features.Accounts.Internal;

internal sealed class AccountService : IAccountService
{
    private readonly IAccountRepository _repository;

    public AccountService(IAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AccountResponseDTO>> GetAccountsAsync(DateOnly selectedDate)
    {
        return await _repository.GetAllAsync(selectedDate);
    }

    public async Task AddAccountAsync(AccountCreateDTO account)
    {
        if (string.IsNullOrWhiteSpace(account.Name))
        {
            throw new ValidationException("Category name is required.");
        }

        await _repository.AddAsync(account);
    }
}
