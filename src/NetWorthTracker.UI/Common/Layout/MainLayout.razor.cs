namespace NetWorthTracker.UI.Common.Layout;

public partial class MainLayout
{
    private DateOnly _selectedDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);

    internal void OnDateChanged(DateOnly selectedDate)
    {
        _selectedDate = selectedDate;
    }

    internal void NavigatePreviousMonth()
    {
        _selectedDate = _selectedDate.AddMonths(-1);
    }

    internal void NavigateNextMonth()
    {
        _selectedDate = _selectedDate.AddMonths(1);
    }
}
