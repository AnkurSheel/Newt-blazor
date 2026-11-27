using NetWorthTracker.Business.Features.FinancialSummary.Api;
using NetWorthTracker.UI.Components.Charts;

namespace NetWorthTracker.UI.Feature.Dashboard;

internal sealed record TrendChartData(
    IReadOnlyList<ChartData> PercentChange,
    IReadOnlyList<ChartData> AbsoluteChange,
    IReadOnlyList<ChartData> Cumulative)
{
    public static TrendChartData Empty { get; } = new([], [], []);
}

internal static class TrendChartDataMapper
{
    public static TrendChartData Map(IReadOnlyList<TrendDTO> trends)
    {
        return new TrendChartData(
            trends.Select(x => new ChartData(x.SelectedDate, x.NetWorthPercentChange)).ToList(),
            trends.Select(x => new ChartData(x.SelectedDate, (double)x.AbsoluteNetWorthChange)).ToList(),
            trends.Select(x => new ChartData(x.SelectedDate, (double)x.CumulativeNetWorth)).ToList());
    }
}
