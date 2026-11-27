using NetWorthTracker.Business.Results;

namespace NetWorthTracker.Business.Features.MonthlyBalances.Api;

public static class MonthlyBalanceErrors
{
    public static readonly Error AccountNotEligible =
        new("MonthlyBalance.AccountNotEligible", "This account is not active for the selected month.");

    public static readonly Error DuplicateBalance =
        new("MonthlyBalance.DuplicateBalance", "A balance already exists for this account and month.");
}
