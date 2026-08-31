namespace NetWorthTracker.Core.Features.MonthlyBalance;

public class MonthlyBalanceCreateDTO
{
    public MonthlyBalanceCreateDTO(int accountId, DateOnly balanceDate, decimal amount)
    {
        AccountId = accountId;
        BalanceDate = balanceDate;
        Amount = amount;
    }

    public int AccountId { get; init; }

    public DateOnly BalanceDate { get; init; }

    public decimal Amount { get; init; }
}
