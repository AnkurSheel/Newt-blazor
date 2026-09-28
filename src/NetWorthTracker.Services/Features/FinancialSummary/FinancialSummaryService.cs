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
            currentMonthlySummary.SelectedDate,
            CalculateAbsoluteChange(currentMonthlySummary.TotalAssets, previousMonthlySummary.TotalAssets),
            CalculatePercentageChange(currentMonthlySummary.TotalAssets, previousMonthlySummary.TotalAssets),
            CalculateAbsoluteChange(currentMonthlySummary.TotalLiabilities, previousMonthlySummary.TotalLiabilities),
            CalculatePercentageChange(currentMonthlySummary.TotalLiabilities, previousMonthlySummary.TotalLiabilities),
            CalculateAbsoluteChange(currentMonthlySummary.NetWorth, previousMonthlySummary.NetWorth),
            CalculatePercentageChange(currentMonthlySummary.NetWorth, previousMonthlySummary.NetWorth));
    }

    public async Task<IReadOnlyList<TrendDTO>> GetMonthlyTrendsAsync(DateOnly selectedDate)
    {
        IReadOnlyList<MonthlySummaryDTO> monthlySummaries =
            await _monthlyBalanceRepository.GetMonthlySummariesAsync(selectedDate);

        var trends = new List<TrendDTO>();
        var monthlySummary = monthlySummaries[0];
        trends.Add(new TrendDTO(monthlySummary.SelectedDate, monthlySummary.NetWorth, 0, 0));
        for (var i = 1; i < monthlySummaries.Count; i++)
        {
            monthlySummary = monthlySummaries[i];
            var previousMonthlySummary = monthlySummaries[i - 1];
            var trend = new TrendDTO(
                monthlySummary.SelectedDate,
                monthlySummary.NetWorth,
                CalculateAbsoluteChange(monthlySummary.NetWorth, previousMonthlySummary.NetWorth),
                CalculatePercentageChange(monthlySummary.NetWorth, previousMonthlySummary.NetWorth));
            trends.Add(trend);
        }

        return trends;
    }

    public async Task<IReadOnlyList<TrendDTO>> GetYearlyTrendsAsync(DateOnly selectedDate)
    {
        IReadOnlyList<MonthlySummaryDTO> yearlySummaries =
            await _monthlyBalanceRepository.GetYearlySummariesAsync(selectedDate);

        var trends = new List<TrendDTO>();
        var yearlySummary = yearlySummaries[0];
        trends.Add(new TrendDTO(yearlySummary.SelectedDate, yearlySummary.NetWorth, 0, 0));
        for (var i = 1; i < yearlySummaries.Count; i++)
        {
            yearlySummary = yearlySummaries[i];
            var previousYearlySummary = yearlySummaries[i - 1];
            var trend = new TrendDTO(
                yearlySummary.SelectedDate,
                yearlySummary.NetWorth,
                CalculateAbsoluteChange(yearlySummary.NetWorth, previousYearlySummary.NetWorth),
                CalculatePercentageChange(yearlySummary.NetWorth, previousYearlySummary.NetWorth));
            trends.Add(trend);
        }

        return trends;
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
