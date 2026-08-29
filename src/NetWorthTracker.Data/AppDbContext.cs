using Microsoft.EntityFrameworkCore;

using NetWorthTracker.Data.Api;
using NetWorthTracker.Data.Features.Account;
using NetWorthTracker.Data.Features.MonthlyBalance;

namespace NetWorthTracker.Data;

public class AppDbContext : DbContext
{
    private readonly IAppPaths _paths;

    public AppDbContext(IAppPaths paths)
    {
        _paths = paths;
    }

    public DbSet<AccountEntity> Accounts => Set<AccountEntity>();

    public DbSet<MonthlyBalanceEntity> MonthlyBalances => Set<MonthlyBalanceEntity>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={_paths.DatabaseFilePath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
