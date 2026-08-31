using Microsoft.Extensions.DependencyInjection;

using NetWorthTracker.Data.Api.Features.Account;
using NetWorthTracker.Data.Api.Features.MonthlyBalance;
using NetWorthTracker.Data.Features.Account;
using NetWorthTracker.Data.Features.MonthlyBalance;

namespace NetWorthTracker.Data;

public static class ServiceRegistry
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IMonthlyBalanceRepository, MonthlyBalanceRepository>();
    }
}
