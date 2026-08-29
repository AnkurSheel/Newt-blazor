using NetWorthTracker.Core.Features.Account;

namespace NetWorthTracker.Data.Api.Features.Account;

public interface IAccountRepository
{
    Task<IReadOnlyList<AccountResponseDTO>> GetAllAsync();

    Task AddAsync(AccountCreateDTO account);
}
