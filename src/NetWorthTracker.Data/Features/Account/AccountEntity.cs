using NetWorthTracker.Core.Features.Account;

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
        ClosedDate = closedDate;
        OpenDate = openDate;
    }

    public int Id { get; private set; }

    public string Name { get; private set; }

    public AccountType Type { get; private set; }

    public DateOnly OpenDate { get; private set; }

    public DateOnly? ClosedDate { get; private set; }
}
