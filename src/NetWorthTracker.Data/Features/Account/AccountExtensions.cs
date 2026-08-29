using NetWorthTracker.Core.Features.Account;

namespace NetWorthTracker.Data.Features.Account;

public static class AccountExtensions
{
    public static AccountResponseDTO ToModel(this AccountEntity accountEntity)
        => new(
            accountEntity.Id,
            accountEntity.Name,
            Enum.Parse<AccountType>(accountEntity.Type),
            accountEntity.OpenDate,
            accountEntity.ClosedDate);

    public static AccountEntity ToEntity(this AccountCreateDTO account)
        => new(
            account.Name,
            account.Type.ToString(),
            account.OpenDate,
            account.ClosedDate);
}

