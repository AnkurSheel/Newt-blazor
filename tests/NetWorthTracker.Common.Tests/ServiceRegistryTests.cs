using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetWorthTracker.Business.Features.Accounts.Api;
using NetWorthTracker.Business.Features.FinancialSummary.Api;
using NetWorthTracker.Business.Features.MonthlyBalances.Api;
using NetWorthTracker.Common;
using NetWorthTracker.Data.Api;

namespace NetWorthTracker.Common.Tests;

public sealed class ServiceRegistryTests
{
    [Fact]
    public void CommonServiceRegistry_ResolvesBusinessServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IAppPaths, DefaultAppPaths>();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        ServiceRegistry.ConfigureServices(services, configuration);
        using var provider = services.BuildServiceProvider();
        var accountService = provider.GetService<IAccountService>();
        var monthlyBalanceService = provider.GetService<IMonthlyBalanceService>();
        var financialSummaryService = provider.GetService<IFinancialSummaryService>();

        // Assert
        Assert.Multiple(
            () => Assert.NotNull(accountService),
            () => Assert.NotNull(monthlyBalanceService),
            () => Assert.NotNull(financialSummaryService));
    }
}
