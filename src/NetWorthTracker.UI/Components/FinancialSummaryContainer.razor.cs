using Microsoft.AspNetCore.Components;

using NetWorthTracker.Core.Features.FinancialSummary;

namespace NetWorthTracker.UI.Components;

public partial class FinancialSummaryContainer : ComponentBase
{
    private DateOnly _lastFetchedDate;
    private MonthlySummaryDTO _monthlySummary = new(0, 0);

    [Parameter]
    public DateOnly SelectedDate { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (SelectedDate != _lastFetchedDate)
        {
            _lastFetchedDate = SelectedDate;
            await LoadSummaryAsync();
        }
    }

    private async Task LoadSummaryAsync()
    {
        _monthlySummary = await FinancialSummaryService.GetMonthlySummary(SelectedDate);
    }
}
