using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Syncfusion.Blazor;
using Syncfusion.Licensing;

namespace NetWorthTracker.UI;

public static class ServiceRegistry
{
    public static IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddSyncfusionBlazor();
        return services;
    }

    public static void RegisterSyncfusionLicense(IConfiguration configuration)
    {
        var licenseKey = configuration["Syncfusion:LicenseKey"];

        if (!string.IsNullOrWhiteSpace(licenseKey))
        {
            SyncfusionLicenseProvider.RegisterLicense(licenseKey);
        }
    }
}
