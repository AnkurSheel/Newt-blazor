using Microsoft.Extensions.DependencyInjection;

using NetWorthTracker.Services.Api.Features.Account;
using NetWorthTracker.Services.Features.Account;

namespace NetWorthTracker.Services;

public static class ServiceRegistry
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IAccountService, AccountService>();
    }
}
