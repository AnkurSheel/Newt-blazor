using NetWorthTracker.Business.Features.Accounts.Api;
using NetWorthTracker.Data.Features.Account;

namespace NetWorthTracker.Data.Tests.TestHelpers;

public static class AccountTestHelpers
{
    public static async Task<int> AddAccount(
        SqliteTestDatabase database,
        string accountName = "Checking",
        AccountType accountType = AccountType.ASSET,
        DateOnly? openDate = null,
        DateOnly? closedDate = null
    )
    {
        await using var dbContext = await database.CreateDbContextAsync();

        var account = new AccountEntity(accountName, accountType, openDate ?? new DateOnly(2025, 1, 1), closedDate);
        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        return account.Id;
    }

    public static AccountCreateDTO GetAccountCreateDto(
        string accountName = "Checking",
        AccountType accountType = AccountType.ASSET,
        DateOnly? openDate = null,
        DateOnly? closedDate = null
    )
    {
        return new AccountCreateDTO(accountName, accountType, openDate ?? new DateOnly(2025, 1, 1), closedDate);
    }
}
