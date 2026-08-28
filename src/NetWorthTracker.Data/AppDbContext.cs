using Microsoft.EntityFrameworkCore;

using NetWorthTracker.Data.Api;
using NetWorthTracker.Data.Features.Account;

namespace NetWorthTracker.Data;

public class AppDbContext : DbContext

{
    private readonly IAppPaths _paths;

    public AppDbContext(IAppPaths paths)
    {
        _paths = paths;
    }

    public DbSet<AccountEntity> Accounts => Set<AccountEntity>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={_paths.DatabaseFilePath}");
}
