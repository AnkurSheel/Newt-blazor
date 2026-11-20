using Microsoft.Extensions.DependencyInjection;

using NetWorthTracker.Business.Features.Accounts.Persistence;
using NetWorthTracker.Business.Features.FinancialSummary.Persistence;
using NetWorthTracker.Business.Features.MonthlyBalances.Persistence;
using NetWorthTracker.Data.Features.Account;
using NetWorthTracker.Data.Features.FinancialSummary;
using NetWorthTracker.Data.Features.MonthlyBalance;

namespace NetWorthTracker.Data;

public static class ServiceRegistry
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContextFactory<AppDbContext>();
        services.AddDbContext<AppDbContext>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IMonthlyBalanceRepository, MonthlyBalanceRepository>();
        services.AddScoped<IFinancialSummaryRepository, FinancialSummaryRepository>();
    }
}
