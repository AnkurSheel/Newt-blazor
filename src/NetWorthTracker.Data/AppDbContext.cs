using Microsoft.EntityFrameworkCore;

using NetWorthTracker.Data.Api;
using NetWorthTracker.Data.Features.Categories;

namespace NetWorthTracker.Data;

public class AppDbContext : DbContext

{
    private readonly IAppPaths _paths;

    public AppDbContext(IAppPaths paths)
    {
        _paths = paths;
    }

    public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={_paths.DatabaseFilePath}");
}
