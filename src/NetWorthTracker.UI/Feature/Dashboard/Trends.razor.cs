using Microsoft.AspNetCore.Components;

using NetWorthTracker.UI.Components.Charts;

using Syncfusion.Blazor.Charts;

namespace NetWorthTracker.UI.Feature.Dashboard;

public partial class Trends : ComponentBase
{
    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public IReadOnlyList<ChartData> PercentChangeData { get; set; } = new List<ChartData>();

    [Parameter]
    public IReadOnlyList<ChartData> AbsoluteChangeData { get; set; } = new List<ChartData>();

    [Parameter]
    public IReadOnlyList<ChartData> CumulativeData { get; set; } = new List<ChartData>();

    [Parameter]
    public IntervalType IntervalType { get; set; }

    [Parameter]
    public string LabelFormat { get; set; } = string.Empty;

    [Parameter]
    public bool ShowMarkers { get; set; }
}
