using Microsoft.AspNetCore.Components;

namespace NetWorthTracker.UI.Common.Layout;

public partial class MainLayout
{
    private DateOnly _selectedDate;
    private EventCallback<DateOnly> _selectMonth;

    [Inject]
    private TimeProvider Clock { get; set; } = null!;

    protected override void OnInitialized()
    {
        var localNow = Clock.GetLocalNow();
        _selectedDate = new DateOnly(localNow.Year, localNow.Month, 1);
        _selectMonth = EventCallback.Factory.Create<DateOnly>(this, OnDateChanged);
    }

    private void OnDateChanged(DateOnly selectedDate)
    {
        _selectedDate = selectedDate;
    }
}
