using Microsoft.Extensions.DependencyInjection;

using NetWorthTracker.Data.Api.Features.Categories;
using NetWorthTracker.Data.Features.Categories;

namespace NetWorthTracker.Data;

public static class ServiceRegistry
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
    }
}
