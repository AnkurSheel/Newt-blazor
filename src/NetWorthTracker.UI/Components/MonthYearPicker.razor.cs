using Microsoft.AspNetCore.Components;

namespace NetWorthTracker.UI.Components;

public partial class MonthYearPicker
{
    [Parameter]
    public string Label { get; set; } = "Select Date";

    [Parameter]
    public DateOnly? Value { get; set; }

    [Parameter]
    public EventCallback<DateOnly?> ValueChanged { get; set; }

    private async Task OnValueChanged(DateOnly? newValue)
    {
        await ValueChanged.InvokeAsync(newValue);
    }
}
