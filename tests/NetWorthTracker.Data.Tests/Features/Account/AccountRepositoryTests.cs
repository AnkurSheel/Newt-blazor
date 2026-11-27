using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

using NetWorthTracker.Business.Features.Accounts.Api;
using NetWorthTracker.Data.Features.Account;
using NetWorthTracker.Data.Features.MonthlyBalance;
using NetWorthTracker.Data.Tests.TestHelpers;

namespace NetWorthTracker.Data.Tests.Features.Account;

public sealed class AccountRepositoryTests : IAsyncLifetime
{
    private readonly SqliteTestDatabase _database = new();
    private readonly AccountRepository _repository;
    private readonly DateOnly queryDate = new(2026, 1, 1);

    public AccountRepositoryTests()
    {
        _repository = new AccountRepository(_database);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _database.DisposeAsync();
    }

    [Fact]
    public async Task Database_rejects_account_date_that_is_not_first_of_month()
    {
        await using var context = await _database.CreateDbContextAsync();

        await Assert.ThrowsAsync<SqliteException>(() => context.Database.ExecuteSqlRawAsync(
            "INSERT INTO accounts (Name, Type, OpenDate) VALUES ('Invalid', 1, '2026-06-15')"));
    }

    [Fact]
    public async Task Account_is_rejected_when_opening_and_closing_dates_share_month()
    {
        var account = AccountTestHelpers.GetAccountCreateDto(
            openDate: new DateOnly(2026, 6, 5), closedDate: new DateOnly(2026, 6, 25));

        await Assert.ThrowsAsync<DbUpdateException>(() => _repository.AddAsync(account));
    }

    [Fact]
    public async Task Mid_month_opening_and_closing_dates_are_normalized_to_month_start()
    {
        var account = AccountTestHelpers.GetAccountCreateDto(
            openDate: new DateOnly(2025, 1, 15), closedDate: new DateOnly(2026, 6, 27));

        await _repository.AddAsync(account);

        var saved = Assert.Single(await _repository.GetAllAsync(queryDate));
        Assert.Multiple(
            () => Assert.Equal(new DateOnly(2025, 1, 1), saved.OpenDate),
            () => Assert.Equal(new DateOnly(2026, 6, 1), saved.ClosedDate));
    }

    [Fact]
    public async Task Account_list_is_empty_when_no_accounts_exist()
    {
        IReadOnlyList<AccountResponseDTO> result = await _repository.GetAllAsync(queryDate);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Account_balances_are_null_when_no_balances_exist()
    {
        await AccountTestHelpers.AddAccount(_database);

        IReadOnlyList<AccountResponseDTO> result = await _repository.GetAllAsync(queryDate);

        var account = Assert.Single(result);
        Assert.Multiple(() => Assert.Equal("Checking", account.Name), () => Assert.Null(account.BalanceOnDate));
    }

    [Fact]
    public async Task Balance_is_populated_when_it_exists_on_selected_date()
    {
        var accountId = await AccountTestHelpers.AddAccount(_database);

        await MonthlyBalanceTestHelpers.AddMonthlyBalance(
            _database,
            new MonthlyBalanceEntity(accountId, queryDate, 5432.10m));

        IReadOnlyList<AccountResponseDTO> result = await _repository.GetAllAsync(queryDate);

        var account = Assert.Single(result);
        Assert.Multiple(
            () => Assert.Equal("Checking", account.Name),
            () => Assert.Equal(5432.10m, account.BalanceOnDate));
    }

    [Fact]
    public async Task Balance_is_null_when_only_another_date_has_a_balance()
    {
        var queryDate = new DateOnly(2026, 1, 1);

        var accountId = await AccountTestHelpers.AddAccount(_database);

        await MonthlyBalanceTestHelpers.AddMonthlyBalance(
            _database,
            new MonthlyBalanceEntity(accountId, new DateOnly(2025, 12, 1), 10000.00m));

        IReadOnlyList<AccountResponseDTO> result = await _repository.GetAllAsync(queryDate);

        var account = Assert.Single(result);
        Assert.Multiple(() => Assert.Equal("Checking", account.Name), () => Assert.Null(account.BalanceOnDate));
    }

    [Fact]
    public async Task Account_details_are_returned_for_selected_date()
    {
        var openDate = new DateOnly(2024, 6, 1);
        var closedDate = new DateOnly(2026, 12, 1);
        var queryDate = new DateOnly(2025, 1, 1);

        var accountId = await AccountTestHelpers.AddAccount(
            _database,
            "Savings",
            AccountType.LIABILITY,
            openDate,
            closedDate);

        IReadOnlyList<AccountResponseDTO> result = await _repository.GetAllAsync(queryDate);

        var account = Assert.Single(result);
        Assert.Multiple(
            () => Assert.Equal(accountId, account.Id),
            () => Assert.Equal("Savings", account.Name),
            () => Assert.Equal(AccountType.LIABILITY, account.Type),
            () => Assert.Equal(openDate, account.OpenDate),
            () => Assert.Equal(closedDate, account.ClosedDate),
            () => Assert.Null(account.BalanceOnDate));
    }

    [Fact]
    public async Task Requested_account_is_returned_with_its_details_and_balance()
    {
        var requestedAccountId = await AccountTestHelpers.AddAccount(
            _database,
            "Requested",
            AccountType.ASSET,
            new DateOnly(2024, 6, 1));
        await AccountTestHelpers.AddAccount(
            _database,
            "Other",
            AccountType.LIABILITY,
            new DateOnly(2024, 6, 1));
        await MonthlyBalanceTestHelpers.AddMonthlyBalance(
            _database,
            new MonthlyBalanceEntity(requestedAccountId, queryDate, 5432.10m));

        AccountResponseDTO? result = await _repository.GetByIdAsync(requestedAccountId, queryDate);

        Assert.NotNull(result);
        Assert.Multiple(
            () => Assert.Equal(requestedAccountId, result.Id),
            () => Assert.Equal("Requested", result.Name),
            () => Assert.Equal(AccountType.ASSET, result.Type),
            () => Assert.Equal(5432.10m, result.BalanceOnDate));
    }

    [Fact]
    public async Task No_account_is_returned_when_requested_account_does_not_exist()
    {
        AccountResponseDTO? result = await _repository.GetByIdAsync(42, queryDate);

        Assert.Null(result);
    }

    [Theory]
    [InlineData("Checking", "checking")]
    [InlineData("Checking", "CHECKING")]
    [InlineData("Checking", "cHeCkInG")]
    [InlineData("Épargne", "éPARGNE")]
    public async Task Account_names_are_duplicates_when_they_differ_only_by_case(
        string existingName,
        string duplicateName
    )
    {
        var openDate = new DateOnly(2025, 1, 1);
        await _repository.AddAsync(AccountTestHelpers.GetAccountCreateDto(existingName, openDate: openDate));
        var duplicate = AccountTestHelpers.GetAccountCreateDto(duplicateName, openDate: openDate);

        await Assert.ThrowsAsync<DuplicateAccountException>(() => _repository.AddAsync(duplicate));
    }
}
