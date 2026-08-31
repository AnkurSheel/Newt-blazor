using Microsoft.Extensions.DependencyInjection;

using NetWorthTracker.Services.Api.Features.Account;
using NetWorthTracker.Services.Api.Features.MonthlyBalance;
using NetWorthTracker.Services.Features.Account;
using NetWorthTracker.Services.Features.MonthlyBalance;

namespace NetWorthTracker.Services;

public static class ServiceRegistry
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IMonthlyBalanceService, MonthlyBalanceService>();
    }
}
