using NetWorthTracker.Core.Features.Account;

namespace NetWorthTracker.Data.Features.Account;

public static class AccountExtensions
{
    public static AccountDTO ToModel(this AccountEntity accountEntity)
        => new(
            accountEntity.Id,
            accountEntity.Name,
            Enum.Parse<AccountType>(accountEntity.Type),
            accountEntity.ClosedDate);

    public static AccountEntity ToEntity(this AccountDTO accountDto)
        => new(
            accountDto.Id,
            accountDto.Name,
            accountDto.Type.ToString(),
            accountDto.ClosedDate);
}

