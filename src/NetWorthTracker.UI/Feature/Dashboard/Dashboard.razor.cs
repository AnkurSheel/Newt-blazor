namespace NetWorthTracker.UI.Feature.Dashboard;

public partial class Dashboard
{
    private DateOnly _selectedDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);

    private void OnDateChanged(DateOnly selectedDate)
    {
        _selectedDate = selectedDate;
    }
}
