using Microsoft.AspNetCore.Components;

namespace NetWorthTracker.UI.Components;

public partial class FinancialCardSummary : ComponentBase
{
    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string BadgeText { get; set; } = string.Empty;

    [Parameter]
    public string CssColor { get; set; } = string.Empty;

    [Parameter]
    public decimal Amount { get; set; }

    [Parameter]
    public string BadgeBorderCssColors { get; set; } = string.Empty;
}
