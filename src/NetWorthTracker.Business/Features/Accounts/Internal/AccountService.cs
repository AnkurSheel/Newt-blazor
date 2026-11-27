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

    public async Task<IReadOnlyList<AccountResponseDTO>> GetActiveAccountsAsync(DateOnly selectedDate)
    {
        IReadOnlyList<AccountResponseDTO> accounts = await _repository.GetAllAsync(selectedDate);

        return accounts.Where(account => account.IsActive(selectedDate))
            .ToList();
    }

    public async Task<IReadOnlyList<AccountResponseDTO>> GetClosedAccountsAsync(DateOnly selectedDate)
    {
        IReadOnlyList<AccountResponseDTO> accounts = await _repository.GetAllAsync(selectedDate);

        return accounts.Where(account => account.IsClosedOn(selectedDate))
            .ToArray();
    }

    public async Task<Result> AddAccountAsync(AccountCreateDTO account)
    {
        var validationError = ValidateAccount(account);
        if (validationError is not null)
        {
            return Result.Failure(validationError);
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

    public async Task<bool> IsAccountActive(int accountId, DateOnly selectedDate)
    {
        AccountResponseDTO? account = await _repository.GetByIdAsync(accountId, selectedDate);

        return account?.IsActive(selectedDate) ?? false;
    }

    private static Error? ValidateAccount(AccountCreateDTO account)
    {
        if (string.IsNullOrWhiteSpace(account.Name))
        {
            return AccountErrors.InvalidName;
        }

        if (!IsClosingMonthAfterOpenMonth(account))
        {
            return AccountErrors.InvalidDates;
        }

        return null;
    }

    private static bool IsClosingMonthAfterOpenMonth(AccountCreateDTO account)
    {
        if (!account.ClosedDate.HasValue)
        {
            return true;
        }

        var openingMonth = new DateOnly(account.OpenDate.Year, account.OpenDate.Month, 1);
        var closingDate = account.ClosedDate.Value;
        var closingMonth = new DateOnly(closingDate.Year, closingDate.Month, 1);
        return closingMonth > openingMonth;
    }
}
