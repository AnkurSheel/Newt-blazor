using Microsoft.Extensions.DependencyInjection;

using NetWorthTracker.Services.Api.Features.FinancialSummary;
using NetWorthTracker.Services.Features.FinancialSummary;

namespace NetWorthTracker.Services;

public static class ServiceRegistry
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IFinancialSummaryService, FinancialSummaryService>();
    }
}
