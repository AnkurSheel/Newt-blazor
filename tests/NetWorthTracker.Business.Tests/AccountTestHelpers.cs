using NetWorthTracker.Business.Features.Accounts.Api;

namespace NetWorthTracker.Business.Tests;

public static class AccountTestHelpers
{
    public static AccountResponseDTO GetAccountResponseDto(
        int id,
        string name,
        DateOnly? openDate = null,
        DateOnly? closedDate = null
    )
    {
        return new AccountResponseDTO
        {
            Id = id,
            Name = name,
            Type = AccountType.ASSET,
            OpenDate = openDate ?? new DateOnly(2026, 1, 1),
            ClosedDate = closedDate
        };
    }
}
