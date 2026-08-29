namespace NetWorthTracker.Core.Features.Account;

public record AccountResponseDTO
{
    public AccountResponseDTO(
        int id,
        string name,
        AccountType type,
        DateOnly openDate,
        DateOnly? closedDate
    )
    {
        Id = id;
        Name = name;
        Type = type;
        ClosedDate = closedDate;
        OpenDate = openDate;
    }

    public int Id { get; init; }

    public string Name { get; init; }

    public AccountType Type { get; init; }

    public DateOnly? ClosedDate { get; init; }

    public bool IsClosed => ClosedDate.HasValue;

    public DateOnly OpenDate { get; init; }
}
