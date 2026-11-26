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
    public async Task GetAllAsync_WhenNoAccountsExist_ReturnsEmptyList()
    {
        IReadOnlyList<AccountResponseDTO> result = await _repository.GetAllAsync(queryDate);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_WhenAccountsExistWithoutBalances_ReturnsAccountsWithNullBalance()
    {
        await AccountTestHelpers.AddAccount(_database);

        IReadOnlyList<AccountResponseDTO> result = await _repository.GetAllAsync(queryDate);

        var account = Assert.Single(result);
        Assert.Multiple(() => Assert.Equal("Checking", account.Name), () => Assert.Null(account.BalanceOnDate));
    }

    [Fact]
    public async Task GetAllAsync_WhenBalanceExistsOnSelectedDate_PopulatesBalanceOnDate()
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
    public async Task GetAllAsync_WhenBalanceExistsForDifferentDate_ReturnsNullBalanceOnDate()
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
    public async Task GetAllAsync_MapsAllAccountPropertiesCorrectly()
    {
        var openDate = new DateOnly(2024, 6, 1);
        var closedDate = new DateOnly(2026, 12, 31);
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
    public async Task GetByIdAsync_WhenAccountExists_ReturnsRequestedAccount()
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
    public async Task GetByIdAsync_WhenAccountDoesNotExist_ReturnsNull()
    {
        AccountResponseDTO? result = await _repository.GetByIdAsync(42, queryDate);

        Assert.Null(result);
    }

    [Theory]
    [InlineData("Checking", "checking")]
    [InlineData("Checking", "CHECKING")]
    [InlineData("Checking", "cHeCkInG")]
    [InlineData("Épargne", "éPARGNE")]
    public async Task AddAsync_WhenNameMatchesIgnoringCase_RejectsDuplicate(string existingName, string duplicateName)
    {
        var openDate = new DateOnly(2025, 1, 1);
        await _repository.AddAsync(AccountTestHelpers.GetAccountCreateDto(existingName, openDate: openDate));
        var duplicate = AccountTestHelpers.GetAccountCreateDto(duplicateName, openDate: openDate);

        await Assert.ThrowsAsync<DuplicateAccountException>(() => _repository.AddAsync(duplicate));
    }
}
