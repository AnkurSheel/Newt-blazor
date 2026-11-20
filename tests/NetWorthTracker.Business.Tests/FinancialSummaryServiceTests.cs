using Microsoft.Extensions.DependencyInjection;
using Moq;

using NetWorthTracker.Business;
using NetWorthTracker.Business.Features.FinancialSummary.Api;
using NetWorthTracker.Business.Features.FinancialSummary.Persistence;

namespace NetWorthTracker.Business.Tests;

public sealed class FinancialSummaryServiceTests
{
    [Fact]
    public async Task GetMonthlyNetworthChangeAsync_WithCurrentAndPreviousSummaries_ReturnsExistingChangeValues()
    {
        // Arrange
        var selectedDate = new DateOnly(2026, 4, 1);
        var previousDate = selectedDate.AddMonths(-1);
        var repository = new Mock<IFinancialSummaryRepository>();
        repository.Setup(value => value.GetMonthlySummaryAsync(selectedDate))
            .ReturnsAsync(new MonthlySummaryDTO(selectedDate, 1200m, 300m));
        repository.Setup(value => value.GetMonthlySummaryAsync(previousDate))
            .ReturnsAsync(new MonthlySummaryDTO(previousDate, 1000m, 250m));
        using var provider = CreateProvider(repository);
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IFinancialSummaryService>();

        // Act
        var result = await service.GetMonthlyNetworthChangeAsync(selectedDate);

        // Assert
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
    public async Task GetMonthlyTrendsAsync_WhenPreviousNetWorthIsZero_ReturnsZeroChanges()
    {
        // Arrange
        var previousDate = new DateOnly(2026, 2, 1);
        var selectedDate = new DateOnly(2026, 3, 1);
        var repository = new Mock<IFinancialSummaryRepository>();
        repository.Setup(value => value.GetMonthlySummariesAsync(selectedDate))
            .ReturnsAsync(
            [
                new MonthlySummaryDTO(previousDate, 0m, 0m),
                new MonthlySummaryDTO(selectedDate, 100m, 0m)
            ]);
        using var provider = CreateProvider(repository);
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IFinancialSummaryService>();

        // Act
        var result = await service.GetMonthlyTrendsAsync(selectedDate);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Multiple(
            () => Assert.Equal(0m, result[1].AbsoluteNetWorthChange),
            () => Assert.Equal(0f, result[1].NetWorthPercentChange));
    }

    private static ServiceProvider CreateProvider(Mock<IFinancialSummaryRepository> repository)
    {
        var services = new ServiceCollection();
        services.AddSingleton(repository.Object);
        ServiceRegistry.ConfigureServices(services);
        return services.BuildServiceProvider();
    }
}
