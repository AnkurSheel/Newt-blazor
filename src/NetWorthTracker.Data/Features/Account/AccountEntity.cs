using NetWorthTracker.Business.Features.Accounts.Api;

namespace NetWorthTracker.Data.Features.Account;

public class AccountEntity
{
    public AccountEntity(
        string name,
        AccountType type,
        DateOnly openDate,
        DateOnly? closedDate
    )
    {
        Name = name;
        Type = type;
        OpenDate = new DateOnly(openDate.Year, openDate.Month, 1);
        ClosedDate = closedDate.HasValue
            ? new DateOnly(closedDate.Value.Year, closedDate.Value.Month, 1)
            : null;
    }

    public int Id { get; private set; }

    public string Name { get; private set; }

    public AccountType Type { get; private set; }

    public DateOnly OpenDate { get; private set; }

    public DateOnly? ClosedDate { get; private set; }
}
