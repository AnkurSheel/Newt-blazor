using NetWorthTracker.Data.Features.MonthlyBalance;
using NetWorthTracker.Data.Tests.TestHelpers;

namespace NetWorthTracker.Data.Tests.Features.MonthlyBalance;

[UsesVerify]
public sealed class MonthlyBalanceQuerySqlTests : IAsyncLifetime
{
    private readonly SqliteTestDatabase _database = new();
    private readonly MonthlyBalanceRepository _repository;

    public MonthlyBalanceQuerySqlTests()
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
    public async Task GetMonthlySummariesQuery_GeneratesExpectedSql()
    {
        // Act - Call repository directly
        await _repository.GetMonthlySummariesAsync(new DateOnly(2026, 3, 1));

        // Assert - Verify actual executed SQL
        await Verify(_database.LastExecutedSql);
    }

    [Fact]
    public async Task GetYearlySummariesQuery_GeneratesExpectedSql()
    {
        // Act - Call repository directly
        await _repository.GetYearlySummariesAsync(new DateOnly(2026, 6, 1));

        // Assert - Verify actual executed SQL
        await Verify(_database.LastExecutedSql);
    }
}
