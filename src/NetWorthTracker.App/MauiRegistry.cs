using NetWorthTracker.Data.Api;

namespace NetWorthTracker.App;

public static class MauiRegistry
{
    public static void ConfigureServices(MauiAppBuilder builder)
    {
        builder.Services.Configure<AppPathOptions>(builder.Configuration.GetSection("AppPaths"));

        builder.Services.AddSingleton<IAppPaths, MauiAppPaths>();
    }
}
