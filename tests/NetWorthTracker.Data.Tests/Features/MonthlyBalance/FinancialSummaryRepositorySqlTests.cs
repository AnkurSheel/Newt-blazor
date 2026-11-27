using NetWorthTracker.Data.Features.FinancialSummary;
using NetWorthTracker.Data.Tests.TestHelpers;

using IAsyncLifetime = Xunit.IAsyncLifetime;

namespace NetWorthTracker.Data.Tests.Features.MonthlyBalance;

[UsesVerify]
public sealed class FinancialSummaryRepositorySqlTests : IAsyncLifetime
{
    private readonly SqliteTestDatabase _database = new();
    private readonly FinancialSummaryRepository _repository;

    public FinancialSummaryRepositorySqlTests()
    {
        _repository = new FinancialSummaryRepository(_database);
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
    public async Task Monthly_summaries_query_generates_expected_sql()
    {
        await _repository.GetMonthlySummariesAsync(new DateOnly(2026, 3, 1));

        await Verify(_database.LastExecutedSql);
    }

    [Fact]
    public async Task Yearly_summaries_query_generates_expected_sql()
    {
        await _repository.GetYearlySummariesAsync(new DateOnly(2026, 6, 1));

        await Verify(_database.LastExecutedSql);
    }
}
