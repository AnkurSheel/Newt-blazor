using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace NetWorthTracker.UI.Components;

public partial class CustomNavLink : ComponentBase
{
    [Parameter]
    public bool IsCollapsed { get; set; }

    [Parameter]
    public required string Link { get; set; }

    [Parameter]
    public required string Title { get; set; }

    [Parameter]
    public RenderFragment? SvgContent { get; set; }

    [Parameter]
    public NavLinkMatch Match { get; set; } = NavLinkMatch.Prefix;
}
