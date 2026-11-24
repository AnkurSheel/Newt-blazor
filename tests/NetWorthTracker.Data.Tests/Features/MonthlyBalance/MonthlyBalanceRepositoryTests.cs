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
    public async Task AddAsync_WithValidMonthlyBalance_PersistsNormalizedMonthDate()
    {
        var accountId = await AccountTestHelpers.AddAccount(_database);

        await AddMonthlyBalance(accountId, new DateOnly(2026, 3, 15), 1500.50m);

        var balance = await _repository.GetBalanceByDateAsync(accountId, new DateOnly(2026, 3, 1));

        Assert.Equal(1500.50m, balance);
    }

    [Fact]
    public async Task GetBalanceByDateAsync_WhenMatchingRecordExists_ReturnsAmount()
    {
        var targetDate = new DateOnly(2026, 2, 1);
        var accountId = await AccountTestHelpers.AddAccount(_database);

        await AddMonthlyBalance(accountId, targetDate, 3200.00m);

        var result = await _repository.GetBalanceByDateAsync(accountId, targetDate);

        Assert.Equal(3200.00m, result);
    }

    [Fact]
    public async Task GetBalanceByDateAsync_WhenNoMatchingRecordExists_ReturnsNull()
    {
        var queryDate = new DateOnly(2026, 2, 1);
        var accountId = await AccountTestHelpers.AddAccount(_database);

        var result = await _repository.GetBalanceByDateAsync(accountId, queryDate);

        Assert.Null(result);
    }

    private async Task AddMonthlyBalance(int accountId, DateOnly balanceDate, decimal amount)
    {
        var createDto = new MonthlyBalanceCreateDTO(accountId, balanceDate, amount);

        await _repository.AddAsync(createDto);
    }
}
