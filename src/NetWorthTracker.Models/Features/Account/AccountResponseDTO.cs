namespace NetWorthTracker.Core.Features.Account;

public record AccountResponseDTO
{
    public required int Id { get; init; }

    public required string Name { get; init; } = string.Empty;

    public AccountType Type { get; init; }

    public DateOnly? ClosedDate { get; init; }

    public DateOnly OpenDate { get; init; }

    public required decimal LatestBalance { get; init; }

    public bool IsClosed => ClosedDate.HasValue;
}
