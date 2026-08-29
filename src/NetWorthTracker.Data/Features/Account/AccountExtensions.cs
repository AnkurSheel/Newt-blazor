#region
using NetWorthTracker.Core.Features.Account;
#endregion

namespace NetWorthTracker.Data.Features.Account;

public static class AccountExtensions
{
    public static AccountResponseDTO ToModel(this AccountEntity accountEntity)
    {
        return new AccountResponseDTO(
            accountEntity.Id,
            accountEntity.Name,
            accountEntity.Type,
            accountEntity.OpenDate,
            accountEntity.ClosedDate);
    }

    public static AccountEntity ToEntity(this AccountCreateDTO account)
    {
        return new AccountEntity(account.Name, account.Type, account.OpenDate, account.ClosedDate);
    }
}
