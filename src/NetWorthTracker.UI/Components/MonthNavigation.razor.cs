using Microsoft.AspNetCore.Components;

namespace NetWorthTracker.UI.Components;

public partial class MonthNavigation
{
    [CascadingParameter]
    public DateOnly SelectedDate { get; set; }

    [CascadingParameter(Name = "SelectMonth")]
    public EventCallback<DateOnly> SelectMonth { get; set; }
}
