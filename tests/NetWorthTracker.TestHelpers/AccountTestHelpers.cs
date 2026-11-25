using Moq;

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

    public static void SetupActiveAccountService(
        Mock<IAccountService> accountService,
        IReadOnlyList<AccountResponseDTO> value
    )
    {
        accountService.Setup(service => service.GetActiveAccountsAsync(It.IsAny<DateOnly>()))
            .ReturnsAsync(value);
    }

    public static void SetupClosedAccountService(
        Mock<IAccountService> accountService,
        IReadOnlyList<AccountResponseDTO> value
    )
    {
        accountService.Setup(service => service.GetClosedAccountsAsync(It.IsAny<DateOnly>()))
            .ReturnsAsync(value);
    }
}
