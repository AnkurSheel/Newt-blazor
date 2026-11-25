using NetWorthTracker.Business.Features.Accounts.Api;

namespace NetWorthTracker.TestHelpers;

public static class AccountTestHelpers
{
    public static AccountResponseDTO CreateAccountResponseDto(
        int id = 1,
        string name = "Test Account",
        DateOnly? openDate = null,
        DateOnly? closedDate = null,
        decimal? balanceOnDate = null,
        AccountType type = AccountType.ASSET
    )
    {
        return new AccountResponseDTO
        {
            Id = id,
            Name = name,
            Type = type,
            OpenDate = openDate ?? new DateOnly(2026, 1, 1),
            ClosedDate = closedDate,
            BalanceOnDate = balanceOnDate
        };
    }
}
