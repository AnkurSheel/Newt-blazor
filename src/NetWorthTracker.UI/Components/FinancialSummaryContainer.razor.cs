using Microsoft.AspNetCore.Components;

using NetWorthTracker.Core.Features.FinancialSummary;

namespace NetWorthTracker.UI.Components;

public partial class FinancialSummaryContainer : ComponentBase
{
    private FinancialSummaryDTO _financialSummary = new(0, 0, 0);

    private DateOnly _lastFetchedDate;

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
        _financialSummary = await FinancialSummaryService.GetFinancialSummaryAsync(SelectedDate);
    }
}
