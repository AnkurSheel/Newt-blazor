namespace NetWorthTracker.Core.Features.Account;

public record AccountDTO(
    int Id,
    string Name,
    AccountType Type,
    DateTime? ClosedDate);
