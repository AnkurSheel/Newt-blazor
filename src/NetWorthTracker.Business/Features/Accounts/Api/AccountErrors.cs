using NetWorthTracker.Business.Results;

namespace NetWorthTracker.Business.Features.Accounts.Api;

public static class AccountErrors
{
    public static Error DuplicateName { get; } = new("Accounts.DuplicateName", "This account has already been added.");

    public static Error InvalidName { get; } = new("Accounts.InvalidName", "Account name is required.");
}
