using NetWorthTracker.Core.Features.Account;
using NetWorthTracker.Data.Features.Account;
using NetWorthTracker.Data.Features.MonthlyBalance;
using NetWorthTracker.Data.Tests.TestHelpers;

namespace NetWorthTracker.Data.Tests.Features.Account;

public sealed class AccountRepositoryTests : IAsyncLifetime
{
    private readonly SqliteTestDatabase _database = new();
    private readonly AccountRepository _repository;

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
        // Arrange
        var queryDate = new DateOnly(2026, 1, 1);

        // Act
        IReadOnlyList<AccountResponseDTO> result = await _repository.GetAllAsync(queryDate);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_WhenAccountsExistWithoutBalances_ReturnsAccountsWithNullBalance()
    {
        // Arrange
        var queryDate = new DateOnly(2026, 1, 1);
        await using var dbContext = await _database.CreateDbContextAsync();
        dbContext.Accounts.Add(new AccountEntity("Checking", AccountType.ASSET, new DateOnly(2025, 1, 1), null));
        await dbContext.SaveChangesAsync();

        // Act
        IReadOnlyList<AccountResponseDTO> result = await _repository.GetAllAsync(queryDate);

        // Assert
        var account = Assert.Single(result);
        Assert.Multiple(() => Assert.Equal("Checking", account.Name), () => Assert.Null(account.BalanceOnDate));
    }

    [Fact]
    public async Task GetAllAsync_WhenBalanceExistsOnSelectedDate_PopulatesBalanceOnDate()
    {
        // Arrange
        var queryDate = new DateOnly(2026, 1, 1);
        await using var dbContext = await _database.CreateDbContextAsync();
        var entity = new AccountEntity("Checking", AccountType.ASSET, new DateOnly(2025, 1, 1), null);
        dbContext.Accounts.Add(entity);
        await dbContext.SaveChangesAsync();

        dbContext.MonthlyBalances.Add(new MonthlyBalanceEntity(entity.Id, queryDate, 5432.10m));
        await dbContext.SaveChangesAsync();

        // Act
        IReadOnlyList<AccountResponseDTO> result = await _repository.GetAllAsync(queryDate);

        // Assert
        var account = Assert.Single(result);
        Assert.Multiple(
            () => Assert.Equal("Checking", account.Name),
            () => Assert.Equal(5432.10m, account.BalanceOnDate));
    }

    [Fact]
    public async Task GetAllAsync_WhenBalanceExistsForDifferentDate_ReturnsNullBalanceOnDate()
    {
        // Arrange
        var balanceDate = new DateOnly(2025, 12, 1);
        var queryDate = new DateOnly(2026, 1, 1);
        await using var dbContext = await _database.CreateDbContextAsync();
        var entity = new AccountEntity("Savings", AccountType.ASSET, new DateOnly(2025, 1, 1), null);
        dbContext.Accounts.Add(entity);
        await dbContext.SaveChangesAsync();

        dbContext.MonthlyBalances.Add(new MonthlyBalanceEntity(entity.Id, balanceDate, 10000.00m));
        await dbContext.SaveChangesAsync();

        // Act
        IReadOnlyList<AccountResponseDTO> result = await _repository.GetAllAsync(queryDate);

        // Assert
        var account = Assert.Single(result);
        Assert.Multiple(() => Assert.Equal("Savings", account.Name), () => Assert.Null(account.BalanceOnDate));
    }

    [Fact]
    public async Task GetAllAsync_OrdersAccountsByIdAscending()
    {
        // Arrange
        var queryDate = new DateOnly(2026, 1, 1);
        await using var dbContext = await _database.CreateDbContextAsync();
        dbContext.Accounts.AddRange(
            new AccountEntity("Checking", AccountType.ASSET, new DateOnly(2025, 1, 1), null),
            new AccountEntity("Mortgage", AccountType.LIABILITY, new DateOnly(2025, 1, 1), null),
            new AccountEntity("Investment", AccountType.ASSET, new DateOnly(2025, 1, 1), null));
        await dbContext.SaveChangesAsync();

        // Act
        IReadOnlyList<AccountResponseDTO> result = await _repository.GetAllAsync(queryDate);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Multiple(() => Assert.True(result[0].Id < result[1].Id), () => Assert.True(result[1].Id < result[2].Id));
    }

    [Fact]
    public async Task GetAllAsync_MapsAllAccountPropertiesCorrectly()
    {
        // Arrange
        var openDate = new DateOnly(2024, 6, 1);
        var closedDate = new DateOnly(2026, 12, 31);
        var queryDate = new DateOnly(2025, 1, 1);
        await using var dbContext = await _database.CreateDbContextAsync();
        var entity = new AccountEntity("Credit Card", AccountType.LIABILITY, openDate, closedDate);
        dbContext.Accounts.Add(entity);
        await dbContext.SaveChangesAsync();

        // Act
        IReadOnlyList<AccountResponseDTO> result = await _repository.GetAllAsync(queryDate);

        // Assert
        var account = Assert.Single(result);
        Assert.Multiple(
            () => Assert.Equal(entity.Id, account.Id),
            () => Assert.Equal("Credit Card", account.Name),
            () => Assert.Equal(AccountType.LIABILITY, account.Type),
            () => Assert.Equal(openDate, account.OpenDate),
            () => Assert.Equal(closedDate, account.ClosedDate),
            () => Assert.Null(account.BalanceOnDate));
    }

    [Fact]
    public async Task AddAsync_WithValidAccountCreateDto_PersistsAccountToDatabase()
    {
        // Arrange
        var createDto = new AccountCreateDTO("Emergency Fund", AccountType.ASSET, new DateOnly(2025, 5, 1), null);

        // Act
        await _repository.AddAsync(createDto);

        // Assert
        IReadOnlyList<AccountResponseDTO> accounts = await _repository.GetAllAsync(new DateOnly(2025, 5, 1));
        var savedAccount = Assert.Single(accounts);
        Assert.Multiple(
            () => Assert.Equal("Emergency Fund", savedAccount.Name),
            () => Assert.Equal(AccountType.ASSET, savedAccount.Type),
            () => Assert.Equal(new DateOnly(2025, 5, 1), savedAccount.OpenDate),
            () => Assert.Null(savedAccount.ClosedDate));
    }
}
