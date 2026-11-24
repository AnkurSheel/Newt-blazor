using Microsoft.Extensions.DependencyInjection;

using NetWorthTracker.Business.Features.Accounts.Api;
using NetWorthTracker.Business.Features.Accounts.Internal;
using NetWorthTracker.Business.Features.FinancialSummary.Api;
using NetWorthTracker.Business.Features.FinancialSummary.Internal;
using NetWorthTracker.Business.Features.MonthlyBalances.Api;
using NetWorthTracker.Business.Features.MonthlyBalances.Internal;

namespace NetWorthTracker.Business;

public static class ServiceRegistry
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IMonthlyBalanceService, MonthlyBalanceService>();
        services.AddScoped<IFinancialSummaryService, FinancialSummaryService>();
    }
}
