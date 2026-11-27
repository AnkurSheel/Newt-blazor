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
    public async Task Account_list_query_generates_expected_sql_with_correlated_balance_subquery()
    {
        await _repository.GetAllAsync(new DateOnly(2026, 1, 1));

        await Verify(_database.LastExecutedSql);
    }

    [Fact]
    public async Task Account_details_query_generates_expected_sql_with_account_and_balance_filters()
    {
        await _repository.GetByIdAsync(42, new DateOnly(2026, 1, 1));

        await Verify(_database.LastExecutedSql);
    }
}
