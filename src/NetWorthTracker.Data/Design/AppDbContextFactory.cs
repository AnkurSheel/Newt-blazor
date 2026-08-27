using Microsoft.EntityFrameworkCore.Design;

namespace NetWorthTracker.Data.Design;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
        => new(new DesignTimeAppPaths());
}