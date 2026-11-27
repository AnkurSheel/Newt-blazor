using NetWorthTracker.Business.Features.FinancialSummary.Api;

namespace NetWorthTracker.UI.Feature.Dashboard;

public partial class Dashboard
{
    private readonly IFinancialSummaryService _financialSummaryService;
    private TrendChartData _monthlyTrendData = TrendChartData.Empty;
    private TrendChartData _yearlyTrendData = TrendChartData.Empty;

    public Dashboard(IFinancialSummaryService financialSummaryService)
    {
        _financialSummaryService = financialSummaryService;
    }

    protected override async Task OnInitializedAsync()
    {
        Task<IReadOnlyList<TrendDTO>> monthlyTrendsAsyncTask = _financialSummaryService.GetMonthlyTrendsAsync(
            new DateOnly(DateTime.Now.Year, DateTime.Now.Month, 1));
        Task<IReadOnlyList<TrendDTO>> yearlyTrendsAsyncTask = _financialSummaryService.GetYearlyTrendsAsync(
            new DateOnly(DateTime.Now.Year, DateTime.Now.Month, 1));

        await Task.WhenAll(monthlyTrendsAsyncTask, yearlyTrendsAsyncTask);
        _monthlyTrendData = TrendChartDataMapper.Map(monthlyTrendsAsyncTask.Result);
        _yearlyTrendData = TrendChartDataMapper.Map(yearlyTrendsAsyncTask.Result);
    }
}
