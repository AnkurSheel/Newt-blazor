using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace NetWorthTracker.UI.Common.Layout;

public partial class NavMenu
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    private bool isCollapsed = true;

    private async Task ToggleNav()
    {
        isCollapsed = !isCollapsed;
        await Task.Delay(50);
        await JSRuntime.InvokeVoidAsync("eval", "window.dispatchEvent(new Event('resize'))");
    }
}
