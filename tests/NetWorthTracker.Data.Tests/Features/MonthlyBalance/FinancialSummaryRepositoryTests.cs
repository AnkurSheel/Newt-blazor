using NetWorthTracker.Business.Features.Accounts.Api;
using NetWorthTracker.Business.Features.FinancialSummary.Api;
using NetWorthTracker.Data.Features.FinancialSummary;
using NetWorthTracker.Data.Features.MonthlyBalance;
using NetWorthTracker.Data.Tests.TestHelpers;

namespace NetWorthTracker.Data.Tests.Features.MonthlyBalance;

public sealed class FinancialSummaryRepositoryTests : IAsyncLifetime
{
    private readonly SqliteTestDatabase _database = new();
    private readonly FinancialSummaryRepository _financialSummaryRepository;

    public FinancialSummaryRepositoryTests()
    {
        _financialSummaryRepository = new FinancialSummaryRepository(_database);
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
    public async Task GetMonthlySummaryAsync_WhenNoBalancesForDate_ReturnsDefaultSummary()
    {
        var queryDate = new DateOnly(2026, 5, 1);

        var result = await _financialSummaryRepository.GetMonthlySummaryAsync(queryDate);

        Assert.Multiple(
            () => Assert.Equal(DateOnly.MinValue, result.SelectedDate),
            () => Assert.Equal(0m, result.TotalAssets),
            () => Assert.Equal(0m, result.TotalLiabilities),
            () => Assert.Equal(0m, result.NetWorth));
    }

    [Fact]
    public async Task GetMonthlySummaryAsync_AggregatesAssetsAndLiabilitiesSeparately()
    {
        var queryDate = new DateOnly(2026, 4, 1);

        var checkingAccountId = await AccountTestHelpers.AddAccount(_database);
        var savingsAccountId = await AccountTestHelpers.AddAccount(_database, "Savings");
        var creditCardAccountId1 =
            await AccountTestHelpers.AddAccount(_database, "Credit Card 1", AccountType.LIABILITY);
        var creditCardAccountId2 =
            await AccountTestHelpers.AddAccount(_database, "Credit Card 2", AccountType.LIABILITY);

        await MonthlyBalanceTestHelpers.AddMonthlyBalances(
            _database,
            new List<MonthlyBalanceEntity>
            {
                new(checkingAccountId, queryDate, 5000.00m),
                new(savingsAccountId, queryDate, 15000.00m),
                new(creditCardAccountId1, queryDate, 2000.00m),
                new(creditCardAccountId2, queryDate, 3000.00m)
            });

        var result = await _financialSummaryRepository.GetMonthlySummaryAsync(queryDate);

        Assert.Multiple(
            () => Assert.Equal(queryDate, result.SelectedDate),
            () => Assert.Equal(20000.00m, result.TotalAssets),
            () => Assert.Equal(5000.00m, result.TotalLiabilities),
            () => Assert.Equal(15000.00m, result.NetWorth));
    }

    [Fact]
    public async Task GetMonthlySummaryAsync_WhenAccountOpenedAfterSelectedDate_ExcludesFromSummary()
    {
        var queryDate = new DateOnly(2026, 3, 1);

        var activeAccountId = await AccountTestHelpers.AddAccount(_database, "Active");
        var futureAccountId = await AccountTestHelpers.AddAccount(
            _database,
            "Future",
            openDate: new DateOnly(2026, 4, 1));

        await MonthlyBalanceTestHelpers.AddMonthlyBalances(
            _database,
            new List<MonthlyBalanceEntity>
            {
                new(activeAccountId, queryDate, 1000m),
                new(futureAccountId, queryDate, 5000m)
            });

        var result = await _financialSummaryRepository.GetMonthlySummaryAsync(queryDate);

        Assert.Equal(1000m, result.TotalAssets);
    }

    [Fact]
    public async Task GetMonthlySummaryAsync_WhenAccountClosedOnOrBeforeSelectedDate_ExcludesFromSummary()
    {
        var queryDate = new DateOnly(2026, 3, 1);

        var activeAccountId = await AccountTestHelpers.AddAccount(_database, "Active");
        var closedAccountId = await AccountTestHelpers.AddAccount(
            _database,
            "Closed",
            openDate: new DateOnly(2025, 1, 1),
            closedDate: new DateOnly(2026, 3, 1));

        await MonthlyBalanceTestHelpers.AddMonthlyBalances(
            _database,
            new List<MonthlyBalanceEntity>
            {
                new(activeAccountId, queryDate, 2000m),
                new(closedAccountId, queryDate, 8000m)
            });

        var result = await _financialSummaryRepository.GetMonthlySummaryAsync(queryDate);

        Assert.Equal(2000m, result.TotalAssets);
    }

    [Fact]
    public async Task GetMonthlySummariesAsync_FiltersUpToSelectedDateAndOrdersByMonthAscending()
    {
        var cutoffDate = new DateOnly(2025, 3, 1);

        var assetAccountId = await AccountTestHelpers.AddAccount(_database, "Asset");
        var liabilityAccountId = await AccountTestHelpers.AddAccount(_database, "Debt", AccountType.LIABILITY);

        await MonthlyBalanceTestHelpers.AddMonthlyBalances(
            _database,
            new List<MonthlyBalanceEntity>
            {
                // Month 1
                new(assetAccountId, new DateOnly(2025, 1, 1), 10000m),
                new(liabilityAccountId, new DateOnly(2025, 1, 1), 2000m),
                // Month 2
                new(assetAccountId, new DateOnly(2025, 2, 1), 12000m),
                new(liabilityAccountId, new DateOnly(2025, 2, 1), 1800m),
                // Month 3
                new(assetAccountId, new DateOnly(2025, 3, 1), 15000m),
                new(liabilityAccountId, new DateOnly(2025, 3, 1), 1500m),
                // Future month (should be excluded)
                new(assetAccountId, new DateOnly(2025, 4, 1), 20000m),
                new(liabilityAccountId, new DateOnly(2025, 4, 1), 1000m)
            });

        IReadOnlyList<MonthlySummaryDTO> result =
            await _financialSummaryRepository.GetMonthlySummariesAsync(cutoffDate);

        Assert.Equal(3, result.Count);
        Assert.Multiple(
            () => Assert.Equal(new DateOnly(2025, 1, 1), result[0].SelectedDate),
            () => Assert.Equal(10000m, result[0].TotalAssets),
            () => Assert.Equal(2000m, result[0].TotalLiabilities),
            () => Assert.Equal(new DateOnly(2025, 2, 1), result[1].SelectedDate),
            () => Assert.Equal(12000m, result[1].TotalAssets),
            () => Assert.Equal(1800m, result[1].TotalLiabilities),
            () => Assert.Equal(new DateOnly(2025, 3, 1), result[2].SelectedDate),
            () => Assert.Equal(15000m, result[2].TotalAssets),
            () => Assert.Equal(1500m, result[2].TotalLiabilities));
    }

    [Fact]
    public async Task GetYearlySummariesAsync_ReturnsLatestMonthSummaryForEachYearUpToSelectedDate()
    {
        var cutoffDate = new DateOnly(2026, 6, 1);

        var assetAccountId = await AccountTestHelpers.AddAccount(
            _database,
            "Asset",
            openDate: new DateOnly(2024, 1, 1));
        var liabilityAccountId = await AccountTestHelpers.AddAccount(
            _database,
            "Liability",
            AccountType.LIABILITY,
            new DateOnly(2024, 1, 1));

        await MonthlyBalanceTestHelpers.AddMonthlyBalances(
            _database,
            new List<MonthlyBalanceEntity>
            {
                // 2024 - Dec is latest
                new(assetAccountId, new DateOnly(2024, 6, 1), 1000m),
                new(liabilityAccountId, new DateOnly(2024, 6, 1), 500m),
                new(assetAccountId, new DateOnly(2024, 12, 1), 2000m),
                new(liabilityAccountId, new DateOnly(2024, 12, 1), 400m),

                // 2025 - Dec is latest
                new(assetAccountId, new DateOnly(2025, 6, 1), 3000m),
                new(liabilityAccountId, new DateOnly(2025, 6, 1), 300m),
                new(assetAccountId, new DateOnly(2025, 12, 1), 4000m),
                new(liabilityAccountId, new DateOnly(2025, 12, 1), 200m),

                // 2026 - Jun is latest up to cutoff
                new(assetAccountId, new DateOnly(2026, 3, 1), 5000m),
                new(liabilityAccountId, new DateOnly(2026, 3, 1), 150m),
                new(assetAccountId, new DateOnly(2026, 6, 1), 6000m),
                new(liabilityAccountId, new DateOnly(2026, 6, 1), 100m),

                // 2026 - Dec is after cutoff (should be ignored)
                new(assetAccountId, new DateOnly(2026, 12, 1), 7000m),
                new(liabilityAccountId, new DateOnly(2026, 12, 1), 50m)
            });

        IReadOnlyList<MonthlySummaryDTO> result = await _financialSummaryRepository.GetYearlySummariesAsync(cutoffDate);

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
