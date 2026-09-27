using Microsoft.AspNetCore.Components;

using NetWorthTracker.Core.Features.FinancialSummary;

namespace NetWorthTracker.UI.Components;

public partial class FinancialSummaryContainer : ComponentBase
{
    private DateOnly _lastFetchedDate;
    private MonthlyNetworthChangeDTO _monthlyNetworthChange = MonthlyNetworthChangeDTO.Default;
    private MonthlySummaryDTO _monthlySummary = MonthlySummaryDTO.Default;

    [Parameter]
    public DateOnly SelectedDate { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (SelectedDate != _lastFetchedDate)
        {
            _lastFetchedDate = SelectedDate;
            await LoadSummaryAsync();
            await LoadMonthlyNetworthChangeAsync();
        }
    }

    private async Task LoadSummaryAsync()
    {
        _monthlySummary = await FinancialSummaryService.GetMonthlySummary(SelectedDate);
    }

    private async Task LoadMonthlyNetworthChangeAsync()
    {
        _monthlyNetworthChange = await FinancialSummaryService.GetMonthlyNetworthChangeAsync(SelectedDate);
    }
}
