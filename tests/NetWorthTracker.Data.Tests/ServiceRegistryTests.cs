using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetWorthTracker.Data.Api;
using NetWorthTracker.Data.Api.Features.Account;
using NetWorthTracker.Data.Api.Features.MonthlyBalance;
using NetWorthTracker.Data.Features.Account;
using NetWorthTracker.Data.Features.MonthlyBalance;
using NetWorthTracker.Data.Tests.TestHelpers;

namespace NetWorthTracker.Data.Tests;

public sealed class ServiceRegistryTests
{
    [Fact]
    public void ConfigureServices_RegistersAllDataServicesAndRepositories()
    {
        // Arrange
        var services = new ServiceCollection();
        var testPaths = new SqliteTestDatabase();
        services.AddSingleton<IAppPaths>(testPaths);

        // Act
        ServiceRegistry.ConfigureServices(services);
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        // Assert
        var dbContextFactory = scope.ServiceProvider.GetService<IDbContextFactory<AppDbContext>>();
        var dbContext = scope.ServiceProvider.GetService<AppDbContext>();
        var accountRepository = scope.ServiceProvider.GetService<IAccountRepository>();
        var monthlyBalanceRepository = scope.ServiceProvider.GetService<IMonthlyBalanceRepository>();

        Assert.Multiple(
            () => Assert.NotNull(dbContextFactory),
            () => Assert.NotNull(dbContext),
            () => Assert.NotNull(accountRepository),
            () => Assert.IsType<AccountRepository>(accountRepository),
            () => Assert.NotNull(monthlyBalanceRepository),
            () => Assert.IsType<MonthlyBalanceRepository>(monthlyBalanceRepository));
    }
}
