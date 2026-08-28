namespace NetWorthTracker.Core.Features.Account;

public record AccountDTO
{
    public AccountDTO(int Id,
        string Name,
        AccountType Type,
        DateTime? ClosedDate)
    {
        this.Id = Id;
        this.Name = Name;
        this.Type = Type;
        this.ClosedDate = ClosedDate;
    }

    public int Id { get; init; }

    public string Name { get; init; }

    public AccountType Type { get; init; }

    public DateTime? ClosedDate { get; init; }
    
    public bool IsClosed => ClosedDate.HasValue;
}
