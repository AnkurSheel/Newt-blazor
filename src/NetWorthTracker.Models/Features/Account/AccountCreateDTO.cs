namespace NetWorthTracker.Core.Features.Account;

public record AccountCreateDTO
{
    public AccountCreateDTO(
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

    public string Name { get; init; }

    public AccountType Type { get; init; }

    public DateOnly? ClosedDate { get; init; }

    public DateOnly OpenDate { get; init; }
}
