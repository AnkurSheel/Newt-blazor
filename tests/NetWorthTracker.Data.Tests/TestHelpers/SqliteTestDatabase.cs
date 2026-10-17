using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using NetWorthTracker.Data.Api;

namespace NetWorthTracker.Data.Tests.TestHelpers;

public sealed class SqliteTestDatabase : IDbContextFactory<AppDbContext>, IAppPaths, IAsyncDisposable, IDisposable
{
    private readonly SqliteConnection _keepAliveConnection;
    private readonly SqlCaptureInterceptor _interceptor = new();

    public string DatabaseFilePath { get; }

    public string? LastExecutedSql => _interceptor.LastCommandText;

    public IReadOnlyList<string> ExecutedCommands => _interceptor.ExecutedCommands;

    public SqliteTestDatabase()
    {
        var dbName = $"InMemoryTest_{Guid.NewGuid():N}";
        DatabaseFilePath = $"{dbName};Mode=Memory;Cache=Shared";
        _keepAliveConnection = new SqliteConnection($"Data Source={DatabaseFilePath}");
        _keepAliveConnection.Open();

        using var dbContext = CreateDbContext();
        dbContext.Database.EnsureCreated();
        _interceptor.Clear();
    }

    public void ClearCapturedSql()
    {
        _interceptor.Clear();
    }

    public AppDbContext CreateDbContext()
    {
        return new TestAppDbContext(this, _interceptor);
    }

    public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<AppDbContext>(new TestAppDbContext(this, _interceptor));
    }

    public void Dispose()
    {
        _keepAliveConnection.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _keepAliveConnection.DisposeAsync();
    }

    private sealed class TestAppDbContext : AppDbContext
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
}
