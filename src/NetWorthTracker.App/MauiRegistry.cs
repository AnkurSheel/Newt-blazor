using NetWorthTracker.Common;
using NetWorthTracker.Data.Api;

namespace NetWorthTracker.App;

public static class MauiRegistry
{
    public static void ConfigureServices(MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<IAppPaths, MauiAppPaths>();
    }
}
