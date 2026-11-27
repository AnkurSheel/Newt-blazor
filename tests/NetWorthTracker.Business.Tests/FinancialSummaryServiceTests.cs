using Moq;

using NetWorthTracker.Business.Features.FinancialSummary.Api;
using NetWorthTracker.Business.Features.FinancialSummary.Internal;
using NetWorthTracker.Business.Features.FinancialSummary.Persistence;

namespace NetWorthTracker.Business.Tests;

public sealed class FinancialSummaryServiceTests
{
    private readonly Mock<IFinancialSummaryRepository> _financialSummaryRepository;
    private readonly FinancialSummaryService _financialSummaryService;

    public FinancialSummaryServiceTests()
    {
        _financialSummaryRepository = new Mock<IFinancialSummaryRepository>();
        _financialSummaryService = new FinancialSummaryService(_financialSummaryRepository.Object);
    }

    [Fact]
    public async Task Monthly_changes_are_calculated_from_current_and_previous_summaries()
    {
        var selectedDate = new DateOnly(2026, 4, 1);
        var previousDate = selectedDate.AddMonths(-1);
        _financialSummaryRepository.Setup(value => value.GetMonthlySummaryAsync(selectedDate))
            .ReturnsAsync(new MonthlySummaryDTO(selectedDate, 1200m, 300m));
        _financialSummaryRepository.Setup(value => value.GetMonthlySummaryAsync(previousDate))
            .ReturnsAsync(new MonthlySummaryDTO(previousDate, 1000m, 250m));

        var result = await _financialSummaryService.GetMonthlyNetworthChangeAsync(selectedDate);

        Assert.Multiple(
            () => Assert.Equal(selectedDate, result.SelectedDate),
            () => Assert.Equal(200m, result.AbsoluteAssetChange),
            () => Assert.Equal(0.2f, result.AssetPercentChange),
            () => Assert.Equal(50m, result.AbsoluteLiabilityChange),
            () => Assert.Equal(0.2f, result.LiabilityPercentChange),
            () => Assert.Equal(150m, result.AbsoluteNetWorthChange),
            () => Assert.Equal(0.2f, result.NetWorthPercentChange));
    }

    [Fact]
    public async Task Changes_are_zero_when_previous_net_worth_is_zero()
    {
        var previousDate = new DateOnly(2026, 2, 1);
        var selectedDate = new DateOnly(2026, 3, 1);
        _financialSummaryRepository.Setup(value => value.GetMonthlySummariesAsync(selectedDate))
            .ReturnsAsync(
            [
                new MonthlySummaryDTO(previousDate, 0m, 0m),
                new MonthlySummaryDTO(selectedDate, 100m, 0m)
            ]);
        IReadOnlyList<TrendDTO> result = await _financialSummaryService.GetMonthlyTrendsAsync(selectedDate);

        Assert.Multiple(
            () => Assert.Equal(2, result.Count),
            () => Assert.Equal(0m, result[1].AbsoluteNetWorthChange),
            () => Assert.Equal(0f, result[1].NetWorthPercentChange));
    }

    [Fact]
    public async Task Yearly_net_worth_changes_are_returned_in_order()
    {
        var firstDate = new DateOnly(2024, 12, 1);
        var secondDate = new DateOnly(2025, 12, 1);
        var selectedDate = new DateOnly(2026, 6, 1);
        _financialSummaryRepository.Setup(value => value.GetYearlySummariesAsync(selectedDate))
            .ReturnsAsync(
            [
                new MonthlySummaryDTO(firstDate, 120m, 20m),
                new MonthlySummaryDTO(secondDate, 180m, 30m),
                new MonthlySummaryDTO(selectedDate, 100m, 10m)
            ]);

        IReadOnlyList<TrendDTO> result = await _financialSummaryService.GetYearlyTrendsAsync(selectedDate);

        Assert.Multiple(
            () => Assert.Equal(3, result.Count),
            () => Assert.Equal(firstDate, result[0].SelectedDate),
            () => Assert.Equal(100m, result[0].CumulativeNetWorth),
            () => Assert.Equal(0m, result[0].AbsoluteNetWorthChange),
            () => Assert.Equal(0f, result[0].NetWorthPercentChange),
            () => Assert.Equal(secondDate, result[1].SelectedDate),
            () => Assert.Equal(150m, result[1].CumulativeNetWorth),
            () => Assert.Equal(50m, result[1].AbsoluteNetWorthChange),
            () => Assert.Equal(0.5f, result[1].NetWorthPercentChange),
            () => Assert.Equal(selectedDate, result[2].SelectedDate),
            () => Assert.Equal(90m, result[2].CumulativeNetWorth),
            () => Assert.Equal(-60m, result[2].AbsoluteNetWorthChange),
            () => Assert.Equal(-0.4f, result[2].NetWorthPercentChange));
    }

    [Fact]
    public async Task Monthly_trends_are_empty_when_no_summaries_exist()
    {
        var selectedDate = new DateOnly(2026, 6, 1);
        _financialSummaryRepository.Setup(value => value.GetMonthlySummariesAsync(selectedDate))
            .ReturnsAsync(Array.Empty<MonthlySummaryDTO>());

        IReadOnlyList<TrendDTO> result = await _financialSummaryService.GetMonthlyTrendsAsync(selectedDate);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Yearly_trends_are_empty_when_no_summaries_exist()
    {
        var selectedDate = new DateOnly(2026, 6, 1);
        _financialSummaryRepository.Setup(value => value.GetYearlySummariesAsync(selectedDate))
            .ReturnsAsync(Array.Empty<MonthlySummaryDTO>());

        IReadOnlyList<TrendDTO> result = await _financialSummaryService.GetYearlyTrendsAsync(selectedDate);

        Assert.Empty(result);
    }
}
