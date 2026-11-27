using Bunit;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NetWorthTracker.Business.Features.FinancialSummary.Api;
using NetWorthTracker.UI.Components;
using NetWorthTracker.UI.Components.Charts;
using NetWorthTracker.UI.Feature.Dashboard;

namespace NetWorthTracker.UI.Tests.Feature.Dashboard;

public sealed class DashboardTests : BunitContext
{
    private readonly Mock<IFinancialSummaryService> _financialSummaryService = new();

    public DashboardTests()
    {
        Services.AddSingleton(_financialSummaryService.Object);
        ComponentFactories.AddStub<MonthNavigation>();
        ComponentFactories.AddStub<FinancialSummaryContainer>();
        ComponentFactories.AddStub<ChangeBarChart>();
        ComponentFactories.AddStub<CumulativeAreaChart>();
    }

    [Fact]
    public async Task Monthly_and_yearly_charts_show_large_net_worth_values_without_losing_dollars()
    {
        SetupMonthlyTrends([new TrendDTO(new DateOnly(2026, 6, 1), 16777217m, -16777217m, -0.25f)]);
        SetupYearlyTrends([new TrendDTO(new DateOnly(2025, 12, 1), 33554433m, 16777217m, 0.5f)]);

        var cut = Render<UI.Feature.Dashboard.Dashboard>();

        await cut.WaitForAssertionAsync(() =>
        {
            var yearly = cut.FindComponents<Trends>()[0].Instance;
            var monthly = cut.FindComponents<Trends>()[1].Instance;
            Assert.Multiple(
                () => Assert.Equal(new DateOnly(2026, 6, 1), Assert.Single(monthly.PercentChangeData).Period),
                () => Assert.Equal(-0.25, Assert.Single(monthly.PercentChangeData).Value),
                () => Assert.Equal(-16777217d, Assert.Single(monthly.AbsoluteChangeData).Value),
                () => Assert.Equal(16777217d, Assert.Single(monthly.CumulativeData).Value),
                () => Assert.Equal(new DateOnly(2025, 12, 1), Assert.Single(yearly.PercentChangeData).Period),
                () => Assert.Equal(0.5, Assert.Single(yearly.PercentChangeData).Value),
                () => Assert.Equal(16777217d, Assert.Single(yearly.AbsoluteChangeData).Value),
                () => Assert.Equal(33554433d, Assert.Single(yearly.CumulativeData).Value));
        });
    }

    [Fact]
    public async Task Charts_are_empty_when_no_trends_exist()
    {
        SetupMonthlyTrends([]);
        SetupYearlyTrends([]);

        var cut = Render<UI.Feature.Dashboard.Dashboard>();

        await cut.WaitForAssertionAsync(() =>
        {
            var yearly = cut.FindComponents<Trends>()[0].Instance;
            var monthly = cut.FindComponents<Trends>()[1].Instance;
            Assert.Multiple(
                () => Assert.Empty(monthly.PercentChangeData),
                () => Assert.Empty(monthly.AbsoluteChangeData),
                () => Assert.Empty(monthly.CumulativeData),
                () => Assert.Empty(yearly.PercentChangeData),
                () => Assert.Empty(yearly.AbsoluteChangeData),
                () => Assert.Empty(yearly.CumulativeData));
        });
    }

    [Fact]
    public async Task Monthly_charts_keep_trends_in_the_order_provided()
    {
        SetupMonthlyTrends([
            new TrendDTO(new DateOnly(2026, 6, 1), 200m, -50m, -0.2f),
            new TrendDTO(new DateOnly(2026, 5, 1), 250m, 100m, 0.4f)
        ]);
        SetupYearlyTrends([]);

        var cut = Render<UI.Feature.Dashboard.Dashboard>();

        await cut.WaitForAssertionAsync(() =>
        {
            var monthly = cut.FindComponents<Trends>()[1].Instance;
            Assert.Multiple(
                () => Assert.Equal(
                    [new DateOnly(2026, 6, 1), new DateOnly(2026, 5, 1)],
                    monthly.PercentChangeData.Select(point => point.Period)),
                () => Assert.Equal(
                    [new DateOnly(2026, 6, 1), new DateOnly(2026, 5, 1)],
                    monthly.AbsoluteChangeData.Select(point => point.Period)),
                () => Assert.Equal(
                    [new DateOnly(2026, 6, 1), new DateOnly(2026, 5, 1)],
                    monthly.CumulativeData.Select(point => point.Period)),
                () => Assert.Equal([-50d, 100d], monthly.AbsoluteChangeData.Select(point => point.Value)),
                () => Assert.Equal([200d, 250d], monthly.CumulativeData.Select(point => point.Value)));
        });
    }

    private void SetupMonthlyTrends(IReadOnlyList<TrendDTO> trends)
    {
        _financialSummaryService.Setup(service => service.GetMonthlyTrendsAsync(It.IsAny<DateOnly>()))
            .ReturnsAsync(trends);
    }

    private void SetupYearlyTrends(IReadOnlyList<TrendDTO> trends)
    {
        _financialSummaryService.Setup(service => service.GetYearlyTrendsAsync(It.IsAny<DateOnly>()))
            .ReturnsAsync(trends);
    }
}
