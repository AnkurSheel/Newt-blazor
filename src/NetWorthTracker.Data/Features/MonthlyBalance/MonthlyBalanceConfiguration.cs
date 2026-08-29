using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NetWorthTracker.Data.Features.MonthlyBalance;

public class MonthlyBalanceConfiguration : IEntityTypeConfiguration<MonthlyBalanceEntity>
{
    public void Configure(EntityTypeBuilder<MonthlyBalanceEntity> builder)
    {
        builder.ToTable("monthly_balances");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.AccountId, x.MonthDate })
            .IsUnique();

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
