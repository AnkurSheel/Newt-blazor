using NetWorthTracker.Core.Features.Account;
using NetWorthTracker.Core.Features.FinancialSummary;
using NetWorthTracker.Core.Features.MonthlyBalance;
using NetWorthTracker.Data.Features.Account;
using NetWorthTracker.Data.Features.MonthlyBalance;
using NetWorthTracker.Data.Tests.TestHelpers;

namespace NetWorthTracker.Data.Tests.Features.MonthlyBalance;

public sealed class MonthlyBalanceRepositoryTests : IAsyncLifetime
{
    private readonly SqliteTestDatabase _database = new();
    private readonly MonthlyBalanceRepository _repository;

    public MonthlyBalanceRepositoryTests()
    {
        _repository = new MonthlyBalanceRepository(_database);
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
    public async Task AddAsync_WithValidMonthlyBalance_PersistsNormalizedMonthDate()
    {
        // Arrange
        await using var dbContext = await _database.CreateDbContextAsync();
        var account = new AccountEntity("Checking", AccountType.ASSET, new DateOnly(2025, 1, 1), null);
        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        var createDto = new MonthlyBalanceCreateDTO(account.Id, new DateOnly(2026, 3, 15), 1500.50m);

        // Act
        await _repository.AddAsync(createDto);

        // Assert
        var balance = await _repository.GetBalanceByDateAsync(account.Id, new DateOnly(2026, 3, 1));
        Assert.Equal(1500.50m, balance);
    }

    [Fact]
    public async Task GetBalanceByDateAsync_WhenMatchingRecordExists_ReturnsAmount()
    {
        // Arrange
        var targetDate = new DateOnly(2026, 2, 1);
        await using var dbContext = await _database.CreateDbContextAsync();
        var account = new AccountEntity("Savings", AccountType.ASSET, new DateOnly(2025, 1, 1), null);
        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        dbContext.MonthlyBalances.Add(new MonthlyBalanceEntity(account.Id, targetDate, 3200.00m));
        await dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetBalanceByDateAsync(account.Id, targetDate);

        // Assert
        Assert.Equal(3200.00m, result);
    }

    [Fact]
    public async Task GetBalanceByDateAsync_WhenNoMatchingRecordExists_ReturnsNull()
    {
        // Arrange
        var queryDate = new DateOnly(2026, 2, 1);
        await using var dbContext = await _database.CreateDbContextAsync();
        var account = new AccountEntity("Savings", AccountType.ASSET, new DateOnly(2025, 1, 1), null);
        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetBalanceByDateAsync(account.Id, queryDate);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetMonthlySummaryAsync_WhenNoBalancesForDate_ReturnsDefaultSummary()
    {
        // Arrange
        var queryDate = new DateOnly(2026, 5, 1);

        // Act
        var result = await _repository.GetMonthlySummaryAsync(queryDate);

        // Assert
        Assert.Multiple(
            () => Assert.Equal(DateOnly.MinValue, result.SelectedDate),
            () => Assert.Equal(0m, result.TotalAssets),
            () => Assert.Equal(0m, result.TotalLiabilities),
            () => Assert.Equal(0m, result.NetWorth));
    }

    [Fact]
    public async Task GetMonthlySummaryAsync_AggregatesAssetsAndLiabilitiesSeparately()
    {
        // Arrange
        var queryDate = new DateOnly(2026, 4, 1);
        await using var dbContext = await _database.CreateDbContextAsync();

        var checking = new AccountEntity("Checking", AccountType.ASSET, new DateOnly(2025, 1, 1), null);
        var savings = new AccountEntity("Savings", AccountType.ASSET, new DateOnly(2025, 1, 1), null);
        var creditCard = new AccountEntity("Credit Card", AccountType.LIABILITY, new DateOnly(2025, 1, 1), null);
        dbContext.Accounts.AddRange(checking, savings, creditCard);
        await dbContext.SaveChangesAsync();

        dbContext.MonthlyBalances.AddRange(
            new MonthlyBalanceEntity(checking.Id, queryDate, 5000.00m),
            new MonthlyBalanceEntity(savings.Id, queryDate, 15000.00m),
            new MonthlyBalanceEntity(creditCard.Id, queryDate, 2000.00m));
        await dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetMonthlySummaryAsync(queryDate);

        // Assert
        Assert.Multiple(
            () => Assert.Equal(queryDate, result.SelectedDate),
            () => Assert.Equal(20000.00m, result.TotalAssets),
            () => Assert.Equal(2000.00m, result.TotalLiabilities),
            () => Assert.Equal(18000.00m, result.NetWorth));
    }

    [Fact]
    public async Task GetMonthlySummaryAsync_WhenAccountOpenedAfterSelectedDate_ExcludesFromSummary()
    {
        // Arrange
        var queryDate = new DateOnly(2026, 3, 1);
        await using var dbContext = await _database.CreateDbContextAsync();

        var activeAccount = new AccountEntity("Active", AccountType.ASSET, new DateOnly(2025, 1, 1), null);
        var futureAccount = new AccountEntity("Future", AccountType.ASSET, new DateOnly(2026, 4, 1), null);
        dbContext.Accounts.AddRange(activeAccount, futureAccount);
        await dbContext.SaveChangesAsync();

        dbContext.MonthlyBalances.AddRange(
            new MonthlyBalanceEntity(activeAccount.Id, queryDate, 1000m),
            new MonthlyBalanceEntity(futureAccount.Id, queryDate, 5000m));
        await dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetMonthlySummaryAsync(queryDate);

        // Assert
        Assert.Multiple(() => Assert.Equal(1000m, result.TotalAssets), () => Assert.Equal(0m, result.TotalLiabilities));
    }

    [Fact]
    public async Task GetMonthlySummaryAsync_WhenAccountClosedOnOrBeforeSelectedDate_ExcludesFromSummary()
    {
        // Arrange
        var queryDate = new DateOnly(2026, 3, 1);
        await using var dbContext = await _database.CreateDbContextAsync();

        var activeAccount = new AccountEntity("Active", AccountType.ASSET, new DateOnly(2025, 1, 1), null);
        var closedAccount = new AccountEntity(
            "Closed",
            AccountType.ASSET,
            new DateOnly(2025, 1, 1),
            new DateOnly(2026, 3, 1));
        dbContext.Accounts.AddRange(activeAccount, closedAccount);
        await dbContext.SaveChangesAsync();

        dbContext.MonthlyBalances.AddRange(
            new MonthlyBalanceEntity(activeAccount.Id, queryDate, 2000m),
            new MonthlyBalanceEntity(closedAccount.Id, queryDate, 8000m));
        await dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetMonthlySummaryAsync(queryDate);

        // Assert
        Assert.Multiple(() => Assert.Equal(2000m, result.TotalAssets), () => Assert.Equal(0m, result.TotalLiabilities));
    }

    [Fact]
    public async Task GetMonthlySummariesAsync_FiltersUpToSelectedDateAndOrdersByMonthAscending()
    {
        // Arrange
        var cutoffDate = new DateOnly(2026, 3, 1);
        await using var dbContext = await _database.CreateDbContextAsync();

        var assetAccount = new AccountEntity("Asset", AccountType.ASSET, new DateOnly(2025, 1, 1), null);
        var liabilityAccount = new AccountEntity("Debt", AccountType.LIABILITY, new DateOnly(2025, 1, 1), null);
        dbContext.Accounts.AddRange(assetAccount, liabilityAccount);
        await dbContext.SaveChangesAsync();

        dbContext.MonthlyBalances.AddRange(
            // Month 1
            new MonthlyBalanceEntity(assetAccount.Id, new DateOnly(2026, 1, 1), 10000m),
            new MonthlyBalanceEntity(liabilityAccount.Id, new DateOnly(2026, 1, 1), 2000m),
            // Month 2
            new MonthlyBalanceEntity(assetAccount.Id, new DateOnly(2026, 2, 1), 12000m),
            new MonthlyBalanceEntity(liabilityAccount.Id, new DateOnly(2026, 2, 1), 1800m),
            // Month 3
            new MonthlyBalanceEntity(assetAccount.Id, new DateOnly(2026, 3, 1), 15000m),
            new MonthlyBalanceEntity(liabilityAccount.Id, new DateOnly(2026, 3, 1), 1500m),
            // Future month (should be excluded)
            new MonthlyBalanceEntity(assetAccount.Id, new DateOnly(2026, 4, 1), 20000m),
            new MonthlyBalanceEntity(liabilityAccount.Id, new DateOnly(2026, 4, 1), 1000m));
        await dbContext.SaveChangesAsync();

        // Act
        IReadOnlyList<MonthlySummaryDTO> result = await _repository.GetMonthlySummariesAsync(cutoffDate);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Multiple(
            () => Assert.Equal(new DateOnly(2026, 1, 1), result[0].SelectedDate),
            () => Assert.Equal(10000m, result[0].TotalAssets),
            () => Assert.Equal(2000m, result[0].TotalLiabilities),
            () => Assert.Equal(new DateOnly(2026, 2, 1), result[1].SelectedDate),
            () => Assert.Equal(12000m, result[1].TotalAssets),
            () => Assert.Equal(1800m, result[1].TotalLiabilities),
            () => Assert.Equal(new DateOnly(2026, 3, 1), result[2].SelectedDate),
            () => Assert.Equal(15000m, result[2].TotalAssets),
            () => Assert.Equal(1500m, result[2].TotalLiabilities));
    }

    [Fact]
    public async Task GetYearlySummariesAsync_ReturnsLatestMonthSummaryForEachYearUpToSelectedDate()
    {
        // Arrange
        var cutoffDate = new DateOnly(2026, 6, 1);
        await using var dbContext = await _database.CreateDbContextAsync();

        var asset = new AccountEntity("Asset", AccountType.ASSET, new DateOnly(2024, 1, 1), null);
        var liability = new AccountEntity("Liability", AccountType.LIABILITY, new DateOnly(2024, 1, 1), null);
        dbContext.Accounts.AddRange(asset, liability);
        await dbContext.SaveChangesAsync();

        dbContext.MonthlyBalances.AddRange(
            // 2024 - Dec is latest
            new MonthlyBalanceEntity(asset.Id, new DateOnly(2024, 6, 1), 1000m),
            new MonthlyBalanceEntity(liability.Id, new DateOnly(2024, 6, 1), 500m),
            new MonthlyBalanceEntity(asset.Id, new DateOnly(2024, 12, 1), 2000m),
            new MonthlyBalanceEntity(liability.Id, new DateOnly(2024, 12, 1), 400m),

            // 2025 - Dec is latest
            new MonthlyBalanceEntity(asset.Id, new DateOnly(2025, 6, 1), 3000m),
            new MonthlyBalanceEntity(liability.Id, new DateOnly(2025, 6, 1), 300m),
            new MonthlyBalanceEntity(asset.Id, new DateOnly(2025, 12, 1), 4000m),
            new MonthlyBalanceEntity(liability.Id, new DateOnly(2025, 12, 1), 200m),

            // 2026 - Jun is latest up to cutoff
            new MonthlyBalanceEntity(asset.Id, new DateOnly(2026, 3, 1), 5000m),
            new MonthlyBalanceEntity(liability.Id, new DateOnly(2026, 3, 1), 150m),
            new MonthlyBalanceEntity(asset.Id, new DateOnly(2026, 6, 1), 6000m),
            new MonthlyBalanceEntity(liability.Id, new DateOnly(2026, 6, 1), 100m),

            // 2026 - Dec is after cutoff (should be ignored)
            new MonthlyBalanceEntity(asset.Id, new DateOnly(2026, 12, 1), 7000m),
            new MonthlyBalanceEntity(liability.Id, new DateOnly(2026, 12, 1), 50m));
        await dbContext.SaveChangesAsync();

        // Act
        IReadOnlyList<MonthlySummaryDTO> result = await _repository.GetYearlySummariesAsync(cutoffDate);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Multiple(
            () => Assert.Equal(new DateOnly(2024, 12, 1), result[0].SelectedDate),
            () => Assert.Equal(2000m, result[0].TotalAssets),
            () => Assert.Equal(400m, result[0].TotalLiabilities),
            () => Assert.Equal(new DateOnly(2025, 12, 1), result[1].SelectedDate),
            () => Assert.Equal(4000m, result[1].TotalAssets),
            () => Assert.Equal(200m, result[1].TotalLiabilities),
            () => Assert.Equal(new DateOnly(2026, 6, 1), result[2].SelectedDate),
            () => Assert.Equal(6000m, result[2].TotalAssets),
            () => Assert.Equal(100m, result[2].TotalLiabilities));
    }
}
