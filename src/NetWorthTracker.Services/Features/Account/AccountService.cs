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
