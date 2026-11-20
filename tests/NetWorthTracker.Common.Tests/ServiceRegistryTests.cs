using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetWorthTracker.Business.Features.Accounts.Api;
using NetWorthTracker.Common;
using NetWorthTracker.Data.Api;

namespace NetWorthTracker.Common.Tests;

public sealed class ServiceRegistryTests
{
    [Fact]
    public void CommonServiceRegistry_ResolvesAccountService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IAppPaths, DefaultAppPaths>();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        ServiceRegistry.ConfigureServices(services, configuration);
        using var provider = services.BuildServiceProvider();
        var accountService = provider.GetService<IAccountService>();

        // Assert
        Assert.NotNull(accountService);
    }
}
