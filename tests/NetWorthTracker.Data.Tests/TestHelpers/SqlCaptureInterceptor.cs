using System.Data.Common;

using Microsoft.EntityFrameworkCore.Diagnostics;

namespace NetWorthTracker.Data.Tests.TestHelpers;

public sealed class SqlCaptureInterceptor : DbCommandInterceptor
{
    private readonly List<string> _executedCommands = new();

    public IReadOnlyList<string> ExecutedCommands => _executedCommands;

    public string? LastCommandText => _executedCommands.LastOrDefault();

    public void Clear()
    {
        _executedCommands.Clear();
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result
    )
    {
        _executedCommands.Add(command.CommandText);

        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default
    )
    {
        _executedCommands.Add(command.CommandText);

        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }
}
