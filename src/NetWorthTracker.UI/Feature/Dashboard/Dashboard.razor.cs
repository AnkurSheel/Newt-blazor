using NetWorthTracker.Core.Features.FinancialSummary;
using NetWorthTracker.Services.Api.Features.FinancialSummary;
using NetWorthTracker.UI.Components.Charts;

namespace NetWorthTracker.UI.Feature.Dashboard;

public partial class Dashboard
{
    private readonly IFinancialSummaryService _financialSummaryService;
    private IReadOnlyList<TrendDTO> _monthlyTrendData = new List<TrendDTO>();
    private DateOnly _selectedDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);
    private IReadOnlyList<TrendDTO> _yearlyTrendData = new List<TrendDTO>();

    public Dashboard(IFinancialSummaryService financialSummaryService)
    {
        _financialSummaryService = financialSummaryService;
    }

    private void OnDateChanged(DateOnly selectedDate)
    {
        _selectedDate = selectedDate;
    }

    protected override async Task OnInitializedAsync()
    {
        Task<IReadOnlyList<TrendDTO>> monthlyTrendsAsyncTask = _financialSummaryService.GetMonthlyTrendsAsync(
            new DateOnly(DateTime.Now.Year, DateTime.Now.Month, 1));
        Task<IReadOnlyList<TrendDTO>> yearlyTrendsAsyncTask = _financialSummaryService.GetYearlyTrendsAsync(
            new DateOnly(DateTime.Now.Year, DateTime.Now.Month, 1));

        await Task.WhenAll(monthlyTrendsAsyncTask, yearlyTrendsAsyncTask);
        _monthlyTrendData = monthlyTrendsAsyncTask.Result;
        _yearlyTrendData = yearlyTrendsAsyncTask.Result;
    }

    private IReadOnlyList<ChartData> GetMomPercentChangeData()
    {
        return _monthlyTrendData.Select(x => new ChartData(x.SelectedDate, x.NetWorthPercentChange))
            .ToList();
    }

    private IReadOnlyList<ChartData> GetMomAbsoluteChangeData()
    {
        return _monthlyTrendData.Select(x => new ChartData(x.SelectedDate, (float)x.AbsoluteNetWorthChange))
            .ToList();
    }

    private IReadOnlyList<ChartData> GetMomCumulativeData()
    {
        return _monthlyTrendData.Select(x => new ChartData(x.SelectedDate, (float)x.CumulativeNetWorth))
            .ToList();
    }

    private IReadOnlyList<ChartData> GetYoyPercentChangeData()
    {
        return _yearlyTrendData.Select(x => new ChartData(x.SelectedDate, x.NetWorthPercentChange))
            .ToList();
    }

    private IReadOnlyList<ChartData> GetYoyAbsoluteChangeData()
    {
        return _yearlyTrendData.Select(x => new ChartData(x.SelectedDate, (float)x.AbsoluteNetWorthChange))
            .ToList();
    }

    private IReadOnlyList<ChartData> GetYoyCumulativeData()
    {
        return _yearlyTrendData.Select(x => new ChartData(x.SelectedDate, (float)x.CumulativeNetWorth))
            .ToList();
    }
}
