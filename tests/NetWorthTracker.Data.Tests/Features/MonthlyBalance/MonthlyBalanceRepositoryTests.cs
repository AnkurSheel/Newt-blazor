using NetWorthTracker.Business.Features.MonthlyBalances.Api;
using NetWorthTracker.Business.Features.MonthlyBalances.Persistence;
using NetWorthTracker.Data.Features.MonthlyBalance;
using NetWorthTracker.Data.Tests.TestHelpers;

namespace NetWorthTracker.Data.Tests.Features.MonthlyBalance;

public sealed class MonthlyBalanceRepositoryTests : IAsyncLifetime
{
    private readonly SqliteTestDatabase _database = new();
    private readonly IMonthlyBalanceRepository _repository;

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
    public async Task Balance_is_stored_at_month_start_when_added_with_mid_month_date()
    {
        var accountId = await AccountTestHelpers.AddAccount(_database);

        await AddMonthlyBalance(accountId, new DateOnly(2026, 3, 15), 1500.50m);

        var balance = await _repository.GetBalanceByDateAsync(accountId, new DateOnly(2026, 3, 1));

        Assert.Equal(1500.50m, balance);
    }

    [Fact]
    public async Task Balance_amount_is_returned_when_requested_month_exists()
    {
        var targetDate = new DateOnly(2026, 2, 1);
        var accountId = await AccountTestHelpers.AddAccount(_database);

        await AddMonthlyBalance(accountId, targetDate, 3200.00m);

        var result = await _repository.GetBalanceByDateAsync(accountId, targetDate);

        Assert.Equal(3200.00m, result);
    }

    [Fact]
    public async Task Null_is_returned_when_requested_month_has_no_balance()
    {
        var queryDate = new DateOnly(2026, 2, 1);
        var accountId = await AccountTestHelpers.AddAccount(_database);

        var result = await _repository.GetBalanceByDateAsync(accountId, queryDate);

        Assert.Null(result);
    }

    [Fact]
    public async Task Duplicate_monthly_balance_is_rejected_when_month_already_has_a_balance()
    {
        var balanceDate = new DateOnly(2026, 2, 1);
        var accountId = await AccountTestHelpers.AddAccount(_database);
        await AddMonthlyBalance(accountId, balanceDate, 3200m);

        await Assert.ThrowsAsync<DuplicateMonthlyBalanceException>(
            () => AddMonthlyBalance(accountId, balanceDate.AddDays(10), 3400m));
    }

    private async Task AddMonthlyBalance(int accountId, DateOnly balanceDate, decimal amount)
    {
        var createDto = new MonthlyBalanceCreateDTO(accountId, balanceDate, amount);

        await _repository.AddAsync(createDto);
    }
}
