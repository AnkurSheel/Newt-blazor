using NetWorthTracker.Business.Features.Accounts.Api;
using NetWorthTracker.Business.Features.MonthlyBalances.Api;
using NetWorthTracker.Business.Features.MonthlyBalances.Persistence;
using NetWorthTracker.Business.Results;

namespace NetWorthTracker.Business.Features.MonthlyBalances.Internal;

internal sealed class MonthlyBalanceService : IMonthlyBalanceService
{
    private readonly IAccountService _accountService;
    private readonly IMonthlyBalanceRepository _repository;

    public MonthlyBalanceService(IMonthlyBalanceRepository repository, IAccountService accountService)
    {
        _repository = repository;
        _accountService = accountService;
    }

    public async Task<Result> AddMonthlyBalanceAsync(MonthlyBalanceCreateDTO monthlyBalanceCreateDto)
    {
        var monthDate = new DateOnly(
            monthlyBalanceCreateDto.BalanceDate.Year,
            monthlyBalanceCreateDto.BalanceDate.Month,
            1);
        var isAccountActive = await _accountService.IsAccountActive(
            monthlyBalanceCreateDto.AccountId,
            monthDate);

        if (!isAccountActive)
        {
            return Result.Failure(MonthlyBalanceErrors.AccountNotEligible);
        }

        var monthlyBalanceAlreadyExist = await DoesMonthlyBalanceAlreadyExist(
            monthlyBalanceCreateDto.AccountId,
            monthDate);

        if (monthlyBalanceAlreadyExist)
        {
            return Result.Failure(MonthlyBalanceErrors.DuplicateBalance);
        }

        try
        {
            await _repository.AddAsync(
                new MonthlyBalanceCreateDTO(
                    monthlyBalanceCreateDto.AccountId,
                    monthDate,
                    monthlyBalanceCreateDto.Amount));

            return Result.Success();
        }
        catch (DuplicateMonthlyBalanceException)
        {
            return Result.Failure(MonthlyBalanceErrors.DuplicateBalance);
        }
    }

    public async Task<decimal?> GetBalanceByDateAsync(int accountId, DateOnly monthDate)
    {
        return await _repository.GetBalanceByDateAsync(accountId, monthDate);
    }

    private async Task<bool> DoesMonthlyBalanceAlreadyExist(int accountId, DateOnly selectedDate)
    {
        var monthlyBalance = await GetBalanceByDateAsync(accountId, selectedDate);

        return monthlyBalance != null;
    }
}
