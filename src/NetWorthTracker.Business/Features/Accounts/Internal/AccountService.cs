using NetWorthTracker.Business.Features.Accounts.Api;
using NetWorthTracker.Business.Features.Accounts.Persistence;
using NetWorthTracker.Business.Results;

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

    public async Task<Result> AddAccountAsync(AccountCreateDTO account)
    {
        if (string.IsNullOrWhiteSpace(account.Name))
        {
            return Result.Failure(AccountErrors.InvalidName);
        }

        try
        {
            await _repository.AddAsync(account);

            return Result.Success();
        }
        catch (DuplicateAccountException)
        {
            return Result.Failure(AccountErrors.DuplicateName);
        }
    }
}
