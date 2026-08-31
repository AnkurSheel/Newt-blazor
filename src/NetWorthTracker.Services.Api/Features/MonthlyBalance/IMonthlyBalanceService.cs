using NetWorthTracker.Core.Features.MonthlyBalance;

namespace NetWorthTracker.Services.Api.Features.MonthlyBalance;

public interface IMonthlyBalanceService
{
    Task AddMonthlyBalanceAsync(MonthlyBalanceCreateDTO monthlyBalance);
}
