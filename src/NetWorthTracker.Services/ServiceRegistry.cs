using Microsoft.Extensions.DependencyInjection;

using NetWorthTracker.Services.Api.Features.Categories;
using NetWorthTracker.Services.Features.Categories;

namespace NetWorthTracker.Services;

public static class ServiceRegistry
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ICategoryService, CategoryService>();
    }
}
