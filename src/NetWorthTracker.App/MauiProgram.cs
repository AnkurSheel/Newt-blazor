using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using NetWorthTracker.Common;
using NetWorthTracker.Data;

using ServiceRegistry = NetWorthTracker.Common.ServiceRegistry;

namespace NetWorthTracker.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        ConfigureAppSettings(builder.Configuration);

        builder.UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        MauiRegistry.ConfigureServices(builder);
        ServiceRegistry.ConfigureServices(builder.Services, builder.Configuration);
        UI.ServiceRegistry.ConfigureServices(builder.Services);

        var app = builder.Build();

        RunMigration(app);
        UI.ServiceRegistry.RegisterSyncfusionLicense(builder.Configuration);

        return app;
    }

    private static void ConfigureAppSettings(ConfigurationManager configuration)
    {
        AddJsonConfiguration(configuration, "appsettings.json", false);

#if DEBUG
        AddJsonConfiguration(configuration, "appsettings.Development.json", true);
#endif

        LocalEnvFile.AddToConfiguration(configuration);
    }

    private static void AddJsonConfiguration(ConfigurationManager configuration, string resourceName, bool optional)
    {
        var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(resourceName);

        if (stream is null)
        {
            if (optional)
            {
                return;
            }

            throw new InvalidOperationException($"Missing embedded configuration resource '{resourceName}'.");
        }

        configuration.AddJsonStream(stream);
    }

    private static void RunMigration(MauiApp app)
    {
        using var scope = app.Services.CreateScope();

        scope.ServiceProvider.GetRequiredService<AppDbContext>()
            .Database.Migrate();
    }
}
