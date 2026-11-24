using NetWorthTracker.Data;

namespace NetWorthTracker.App;

public static class MauiRegistry
{
    public static void ConfigureServices(MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<IAppPaths, MauiAppPaths>();
    }
}
