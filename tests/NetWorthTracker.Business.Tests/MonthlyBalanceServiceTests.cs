using Moq;

using NetWorthTracker.Business.Features.Accounts.Api;
using NetWorthTracker.Business.Features.MonthlyBalances.Api;
using NetWorthTracker.Business.Features.MonthlyBalances.Internal;
using NetWorthTracker.Business.Features.MonthlyBalances.Persistence;

namespace NetWorthTracker.Business.Tests;

public sealed class MonthlyBalanceServiceTests
{
    private readonly Mock<IMonthlyBalanceRepository> _repository = new();
    private readonly Mock<IAccountService> _accountService = new();
    private readonly IMonthlyBalanceService _monthlyBalanceService;

    public MonthlyBalanceServiceTests()
    {
        _monthlyBalanceService = new MonthlyBalanceService(_repository.Object, _accountService.Object);
    }

    [Fact]
    public async Task AddMonthlyBalanceAsync_WithActiveAccountAndNoExistingBalance_AddsAtMonthStart()
    {
        var accountId = 12;
        var balanceDate = new DateOnly(2026, 6, 15);
        var monthStart = new DateOnly(2026, 6, 1);
        var amount = 1250m;
        SetupAccountActive(accountId, monthStart, true);
        SetupBalanceForDate(accountId, monthStart, null);

        var result = await _monthlyBalanceService.AddMonthlyBalanceAsync(
            new MonthlyBalanceCreateDTO(accountId, balanceDate, amount));

        Assert.True(result.IsSuccess);
        _repository.Verify(
            repository => repository.AddAsync(It.Is<MonthlyBalanceCreateDTO>(
                balance => balance.AccountId == accountId
                           && balance.BalanceDate == monthStart
                           && balance.Amount == amount)),
            Times.Once);
    }

    [Fact]
    public async Task AddMonthlyBalanceAsync_WhenAccountIsNotActive_ReturnsEligibilityFailureWithoutPersisting()
    {
        var accountId = 12;
        var monthStart = new DateOnly(2026, 6, 1);
        SetupAccountActive(accountId, monthStart, false);

        var result = await _monthlyBalanceService.AddMonthlyBalanceAsync(
            new MonthlyBalanceCreateDTO(accountId, new DateOnly(2026, 6, 15), 1250m));

        Assert.Equal(MonthlyBalanceErrors.AccountNotEligible, result.Error);
        _repository.Verify(repository => repository.AddAsync(It.IsAny<MonthlyBalanceCreateDTO>()), Times.Never);
    }

    [Fact]
    public async Task AddMonthlyBalanceAsync_WhenBalanceAlreadyExists_ReturnsDuplicateFailureWithoutPersisting()
    {
        var accountId = 12;
        var monthStart = new DateOnly(2026, 6, 1);
        SetupAccountActive(accountId, monthStart, true);
        SetupBalanceForDate(accountId, monthStart, 0m);

        var result = await _monthlyBalanceService.AddMonthlyBalanceAsync(
            new MonthlyBalanceCreateDTO(accountId, new DateOnly(2026, 6, 15), 1250m));

        Assert.Equal(MonthlyBalanceErrors.DuplicateBalance, result.Error);
        _repository.Verify(repository => repository.AddAsync(It.IsAny<MonthlyBalanceCreateDTO>()), Times.Never);
    }

    [Fact]
    public async Task AddMonthlyBalanceAsync_WhenConcurrentInsertViolatesUniqueIndex_ReturnsDuplicateFailure()
    {
        var accountId = 12;
        var monthStart = new DateOnly(2026, 6, 1);
        SetupAccountActive(accountId, monthStart, true);
        SetupBalanceForDate(accountId, monthStart, null);
        _repository.Setup(repository => repository.AddAsync(It.IsAny<MonthlyBalanceCreateDTO>()))
            .ThrowsAsync(new DuplicateMonthlyBalanceException(new Exception()));

        var result = await _monthlyBalanceService.AddMonthlyBalanceAsync(
            new MonthlyBalanceCreateDTO(accountId, monthStart, 1250m));

        Assert.Equal(MonthlyBalanceErrors.DuplicateBalance, result.Error);
    }

    private void SetupAccountActive(int accountId, DateOnly selectedDate, bool isActive)
    {
        _accountService.Setup(service => service.IsAccountActive(accountId, selectedDate))
            .ReturnsAsync(isActive);
    }

    private void SetupBalanceForDate(int accountId, DateOnly monthDate, decimal? balance)
    {
        _repository.Setup(repository => repository.GetBalanceByDateAsync(accountId, monthDate))
            .ReturnsAsync(balance);
    }
}
