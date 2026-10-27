using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NetWorthTracker.Common;

public static class ServiceRegistry
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppPathOptions>(configuration.GetSection("AppPaths"));
        Data.ServiceRegistry.ConfigureServices(services);
        Services.ServiceRegistry.ConfigureServices(services);

    }
}
