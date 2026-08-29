#region
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NetWorthTracker.Core.Features.Account;
#endregion

namespace NetWorthTracker.Data.Features.Account;

public class AccountConfiguration : IEntityTypeConfiguration<AccountEntity>
{
    public void Configure(EntityTypeBuilder<AccountEntity> builder)
    {
        builder.ToTable("accounts");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.Name)
            .IsUnique();

        IEnumerable<int> allowedTypes = Enum.GetValues<AccountType>()
            .Where(x => x != AccountType.DONOTUSE)
            .Select(x => (int)x);
        var sqlInClauseValues = string.Join(", ", allowedTypes);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Account_Name_Length", "length([Name]) <= 256");
            t.HasCheckConstraint("CK_Account_Type_Values", $"[Type] IN ({sqlInClauseValues})");
            t.HasCheckConstraint("CK_Account_Dates", "[ClosedDate] IS NULL OR [ClosedDate] > [OpenDate]");
        });
    }
}
