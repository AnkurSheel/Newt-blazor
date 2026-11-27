using Microsoft.AspNetCore.Components;

namespace NetWorthTracker.UI.Components;

public partial class PageHeader
{
    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
