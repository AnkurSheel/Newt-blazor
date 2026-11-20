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

    [Fact]
    public async Task GetYearlyTrendsAsync_WithSummaries_ReturnsOrderedNetWorthChanges()
    {
        // Arrange
        var firstDate = new DateOnly(2024, 12, 1);
        var secondDate = new DateOnly(2025, 12, 1);
        var selectedDate = new DateOnly(2026, 6, 1);
        var repository = new Mock<IFinancialSummaryRepository>();
        repository.Setup(value => value.GetYearlySummariesAsync(selectedDate))
            .ReturnsAsync(
            [
                new MonthlySummaryDTO(firstDate, 120m, 20m),
                new MonthlySummaryDTO(secondDate, 180m, 30m),
                new MonthlySummaryDTO(selectedDate, 100m, 10m)
            ]);
        using var provider = CreateProvider(repository);
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IFinancialSummaryService>();

        // Act
        var result = await service.GetYearlyTrendsAsync(selectedDate);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Multiple(
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
    public async Task GetMonthlyTrendsAsync_WhenNoSummaries_ReturnsEmptyList()
    {
        // Arrange
        var selectedDate = new DateOnly(2026, 6, 1);
        var repository = new Mock<IFinancialSummaryRepository>();
        repository.Setup(value => value.GetMonthlySummariesAsync(selectedDate))
            .ReturnsAsync(Array.Empty<MonthlySummaryDTO>());
        using var provider = CreateProvider(repository);
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IFinancialSummaryService>();

        // Act
        var result = await service.GetMonthlyTrendsAsync(selectedDate);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetYearlyTrendsAsync_WhenNoSummaries_ReturnsEmptyList()
    {
        // Arrange
        var selectedDate = new DateOnly(2026, 6, 1);
        var repository = new Mock<IFinancialSummaryRepository>();
        repository.Setup(value => value.GetYearlySummariesAsync(selectedDate))
            .ReturnsAsync(Array.Empty<MonthlySummaryDTO>());
        using var provider = CreateProvider(repository);
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IFinancialSummaryService>();

        // Act
        var result = await service.GetYearlyTrendsAsync(selectedDate);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FinancialSummaryService_ResolvesSameInstanceWithinScopeAndDifferentInstanceAcrossScopes()
    {
        // Arrange
        using var provider = CreateProvider(new Mock<IFinancialSummaryRepository>());
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();

        // Act
        var first = firstScope.ServiceProvider.GetRequiredService<IFinancialSummaryService>();
        var sameScope = firstScope.ServiceProvider.GetRequiredService<IFinancialSummaryService>();
        var otherScope = secondScope.ServiceProvider.GetRequiredService<IFinancialSummaryService>();

        // Assert
        Assert.Multiple(
            () => Assert.Same(first, sameScope),
            () => Assert.NotSame(first, otherScope));
    }

    private static ServiceProvider CreateProvider(Mock<IFinancialSummaryRepository> repository)
    {
        var services = new ServiceCollection();
        services.AddSingleton(repository.Object);
        ServiceRegistry.ConfigureServices(services);
        return services.BuildServiceProvider();
    }
}
