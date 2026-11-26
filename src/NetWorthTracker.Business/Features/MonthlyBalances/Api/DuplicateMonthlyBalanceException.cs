namespace NetWorthTracker.Business.Features.MonthlyBalances.Api;

public sealed class DuplicateMonthlyBalanceException : Exception
{
    public DuplicateMonthlyBalanceException(Exception innerException)
        : base("A balance already exists for this account and month.", innerException)
    {
    }
}
