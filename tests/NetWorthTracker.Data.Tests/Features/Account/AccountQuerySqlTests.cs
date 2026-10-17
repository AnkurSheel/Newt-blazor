using NetWorthTracker.Data.Features.Account;
using NetWorthTracker.Data.Tests.TestHelpers;

namespace NetWorthTracker.Data.Tests.Features.Account;

[UsesVerify]
public sealed class AccountQuerySqlTests : IAsyncLifetime
{
    private readonly SqliteTestDatabase _database = new();
    private readonly AccountRepository _repository;

    public AccountQuerySqlTests()
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
    public async Task GetAllAccountsQuery_GeneratesExpectedSqlWithCorrelatedSubquery()
    {
        // Act - Call repository directly
        await _repository.GetAllAsync(new DateOnly(2026, 1, 1));

        // Assert - Verify actual executed SQL
        await Verify(_database.LastExecutedSql);
    }
}
