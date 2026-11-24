using NetWorthTracker.Business.Features.Accounts.Api;

namespace NetWorthTracker.Business.Features.Accounts.Persistence;

public interface IAccountRepository
{
    Task<IReadOnlyList<AccountResponseDTO>> GetAllAsync(DateOnly selectedDate);

    Task AddAsync(AccountCreateDTO account);
}
