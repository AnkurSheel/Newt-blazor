namespace NetWorthTracker.UI.Common.Layout;

public partial class MainLayout
{
    private DateOnly _selectedDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);

    private void OnDateChanged(DateOnly selectedDate)
    {
        _selectedDate = selectedDate;
    }
}
