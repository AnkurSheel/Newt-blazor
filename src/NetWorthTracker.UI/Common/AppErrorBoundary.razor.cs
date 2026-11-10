using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;

namespace NetWorthTracker.UI.Common;

public partial class AppErrorBoundary : ErrorBoundary
{
    [Inject]
    private ILogger<AppErrorBoundary> Logger { get; set; } = default!;

    protected override Task OnErrorAsync(Exception exception)
    {
        Logger.LogError(exception, "Unhandled Blazor exception caught by error boundary: {Message}", exception.Message);

        return Task.CompletedTask;
    }
}
