using Microsoft.Extensions.DependencyInjection;

using NetWorthTracker.Data.Api.Features.Account;
using NetWorthTracker.Data.Features.Account;

namespace NetWorthTracker.Data;

public static class ServiceRegistry
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>();
        services.AddScoped<IAccountRepository, AccountRepository>();
    }
}
