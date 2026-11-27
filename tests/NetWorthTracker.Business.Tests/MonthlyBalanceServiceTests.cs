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
    public async Task New_balance_is_saved_at_month_start_for_active_account_without_existing_balance()
    {
        var accountId = 12;
        var balanceDate = new DateOnly(2026, 6, 15);
        var monthStart = new DateOnly(2026, 6, 1);
        var amount = 1250m;
        SetupAccountActive(accountId, monthStart, true);
        SetupBalanceForDate(accountId, monthStart, null);

        var result = await _monthlyBalanceService.AddMonthlyBalanceAsync(
            new MonthlyBalanceCreateDTO(accountId, balanceDate, amount));

        Assert.Multiple(
            () => Assert.True(result.IsSuccess),
            () => _repository.Verify(
                repository => repository.AddAsync(It.Is<MonthlyBalanceCreateDTO>(
                    balance => balance.AccountId == accountId
                               && balance.BalanceDate == monthStart
                               && balance.Amount == amount)),
                Times.Once));
    }

    [Fact]
    public async Task Balance_is_rejected_without_saving_when_account_is_not_active()
    {
        var accountId = 12;
        var monthStart = new DateOnly(2026, 6, 1);
        SetupAccountActive(accountId, monthStart, false);

        var result = await _monthlyBalanceService.AddMonthlyBalanceAsync(
            new MonthlyBalanceCreateDTO(accountId, new DateOnly(2026, 6, 15), 1250m));

        Assert.Multiple(
            () => Assert.Equal(MonthlyBalanceErrors.AccountNotEligible.Code, result.Error.Code),
            () => Assert.Equal(MonthlyBalanceErrors.AccountNotEligible.Message, result.Error.Message),
            () => _repository.Verify(
                repository => repository.AddAsync(It.IsAny<MonthlyBalanceCreateDTO>()),
                Times.Never));
    }

    [Fact]
    public async Task Duplicate_failure_is_returned_without_saving_when_balance_already_exists()
    {
        var accountId = 12;
        var monthStart = new DateOnly(2026, 6, 1);
        SetupAccountActive(accountId, monthStart, true);
        SetupBalanceForDate(accountId, monthStart, 0m);

        var result = await _monthlyBalanceService.AddMonthlyBalanceAsync(
            new MonthlyBalanceCreateDTO(accountId, new DateOnly(2026, 6, 15), 1250m));

        Assert.Multiple(
            () => Assert.Equal(MonthlyBalanceErrors.DuplicateBalance.Code, result.Error.Code),
            () => Assert.Equal(MonthlyBalanceErrors.DuplicateBalance.Message, result.Error.Message),
            () => _repository.Verify(
                repository => repository.AddAsync(It.IsAny<MonthlyBalanceCreateDTO>()),
                Times.Never));
    }

    [Fact]
    public async Task Duplicate_failure_is_returned_when_duplicate_occurs_during_balance_save()
    {
        var accountId = 12;
        var monthStart = new DateOnly(2026, 6, 1);
        SetupAccountActive(accountId, monthStart, true);
        SetupBalanceForDate(accountId, monthStart, null);
        _repository.Setup(repository => repository.AddAsync(It.IsAny<MonthlyBalanceCreateDTO>()))
            .ThrowsAsync(new DuplicateMonthlyBalanceException(new Exception()));

        var result = await _monthlyBalanceService.AddMonthlyBalanceAsync(
            new MonthlyBalanceCreateDTO(accountId, monthStart, 1250m));

        Assert.Multiple(
            () => Assert.Equal(MonthlyBalanceErrors.DuplicateBalance.Code, result.Error.Code),
            () => Assert.Equal(MonthlyBalanceErrors.DuplicateBalance.Message, result.Error.Message));
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
