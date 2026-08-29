using NetWorthTracker.Data.Features.Account;

namespace NetWorthTracker.Data.Features.MonthlyBalance;

public class MonthlyBalanceEntity
{
    public MonthlyBalanceEntity(int accountId, DateOnly monthDate, decimal amount)
    {
        AccountId = accountId;
        MonthDate = new DateOnly(monthDate.Year, monthDate.Month, 1);
        Amount = amount;
    }

    public int Id { get; private set; }

    public int AccountId { get; private set; }

    public AccountEntity Account { get; private set; } = null!;

    public DateOnly MonthDate { get; private set; }

    public decimal Amount { get; private set; }
}
