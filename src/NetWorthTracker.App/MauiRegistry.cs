using NetWorthTracker.Core.Features.Categories;
using NetWorthTracker.Data;
using NetWorthTracker.Data.Api;
using NetWorthTracker.Data.Api.Features.Categories;
using NetWorthTracker.Data.Features.Categories;

namespace NetWorthTracker.App;

public static class MauiRegistry
{
    public static void ConfigureServices(MauiAppBuilder builder)
    {
        builder.Services.Configure<AppPathOptions>(builder.Configuration.GetSection("AppPaths"));

        builder.Services.AddSingleton<IAppPaths, MauiAppPaths>();

        builder.Services.AddDbContext<AppDbContext>();

        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
    }
}
