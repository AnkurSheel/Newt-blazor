namespace NetWorthTracker.Core.Features.Account;

public record AccountResponseDTO
{
    public string Name { get; init; } = string.Empty;

    public AccountType Type { get; init; }

    public DateOnly? ClosedDate { get; init; }

    public DateOnly OpenDate { get; init; }

    public decimal LatestBalance { get; init; }

    public bool IsClosed => ClosedDate.HasValue;
}
