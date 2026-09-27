using Microsoft.AspNetCore.Components;

namespace NetWorthTracker.UI.Components;

public partial class Pill : ComponentBase
{
    [Parameter]
    public string CssClasses { get; set; } = string.Empty;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
