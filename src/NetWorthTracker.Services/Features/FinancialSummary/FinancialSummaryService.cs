using NetWorthTracker.Core.Features.FinancialSummary;
using NetWorthTracker.Data.Api.Features.MonthlyBalance;
using NetWorthTracker.Services.Api.Features.FinancialSummary;

namespace NetWorthTracker.Services.Features.FinancialSummary;

public class FinancialSummaryService : IFinancialSummaryService
{
    private readonly IMonthlyBalanceRepository _monthlyBalanceRepository;

    public FinancialSummaryService(IMonthlyBalanceRepository monthlyBalanceRepository)
    {
        _monthlyBalanceRepository = monthlyBalanceRepository;
    }

    public async Task<MonthlySummaryDTO> GetMonthlySummary(DateOnly selectedDate)
    {
        return await _monthlyBalanceRepository.GetMonthlySummaryAsync(selectedDate);
    }

    public async Task<MonthlyNetworthChangeDTO> GetMonthlyNetworthChangeAsync(DateOnly selectedDate)
    {
        var previousMonthDate = selectedDate.AddMonths(-1);
        Task<MonthlySummaryDTO> currentMonthlySummaryAsyncTask =
            _monthlyBalanceRepository.GetMonthlySummaryAsync(selectedDate);
        Task<MonthlySummaryDTO> previousMonthlySummaryAsyncTask =
            _monthlyBalanceRepository.GetMonthlySummaryAsync(previousMonthDate);

        await Task.WhenAll(currentMonthlySummaryAsyncTask, previousMonthlySummaryAsyncTask);

        var currentMonthlySummary = currentMonthlySummaryAsyncTask.Result;
        var previousMonthlySummary = previousMonthlySummaryAsyncTask.Result;

        return new MonthlyNetworthChangeDTO(
            CalculateAbsoluteChange(currentMonthlySummary.TotalAssets, previousMonthlySummary.TotalAssets),
            CalculatePercentageChange(currentMonthlySummary.TotalAssets, previousMonthlySummary.TotalAssets),
            CalculateAbsoluteChange(currentMonthlySummary.TotalLiabilities, previousMonthlySummary.TotalLiabilities),
            CalculatePercentageChange(currentMonthlySummary.TotalLiabilities, previousMonthlySummary.TotalLiabilities),
            CalculateAbsoluteChange(currentMonthlySummary.NetWorth, previousMonthlySummary.NetWorth),
            CalculatePercentageChange(currentMonthlySummary.NetWorth, previousMonthlySummary.NetWorth));
    }

    private decimal CalculateAbsoluteChange(decimal currentValue, decimal previous)
    {
        if (previous == 0m)
        {
            return 0;
        }

        return currentValue - previous;
    }

    private float CalculatePercentageChange(decimal currentValue, decimal previous)
    {
        if (previous == 0m)
        {
            return 0;
        }
        var actualChange = currentValue - previous;

        return (float)(actualChange / previous);
    }
}
