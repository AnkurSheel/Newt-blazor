using Microsoft.AspNetCore.Components;

using Syncfusion.Blazor.Charts;

namespace NetWorthTracker.UI.Components.Charts;

public partial class CumulativeAreaChart : ComponentBase
{
    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string Format { get; set; } = string.Empty;

    [Parameter]
    public IReadOnlyList<ChartData> Data { get; set; } = new List<ChartData>();

    [Parameter]
    public IntervalType IntervalType { get; set; }

    [Parameter]
    public string LabelFormat { get; set; } = string.Empty;

    [Parameter]
    public bool ShowMarkers { get; set; }
}
