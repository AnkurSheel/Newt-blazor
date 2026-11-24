using Microsoft.EntityFrameworkCore;

namespace NetWorthTracker.Data.Tests.TestHelpers;

public sealed class TestAppDbContext : AppDbContext
{
    private readonly SqlCaptureInterceptor _interceptor;

    public TestAppDbContext(IAppPaths paths, SqlCaptureInterceptor interceptor) : base(paths)
    {
        _interceptor = interceptor;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        base.OnConfiguring(options);
        options.AddInterceptors(_interceptor);
    }
}
