using Microsoft.Extensions.DependencyInjection;

using NetWorthTracker.Business.Features.Accounts.Api;
using NetWorthTracker.Business.Features.Accounts.Internal;

namespace NetWorthTracker.Business;

public static class ServiceRegistry
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IAccountService, AccountService>();
    }
}
