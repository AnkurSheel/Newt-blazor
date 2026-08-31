using Microsoft.AspNetCore.Components;

using NetWorthTracker.Core.Features.MonthlyBalance;

namespace NetWorthTracker.UI.Feature.MonthlyBalance;

public partial class AddMonthlyBalanceModal
{
    private decimal _balance;
    private DateOnly? _transactionDate;

    [Parameter]
    public bool IsVisible { get; set; }

    [Parameter]
    public EventCallback<bool> IsVisibleChanged { get; set; }

    [Parameter]
    public required int AccountId { get; set; }

    [Parameter]
    public required string AccountName { get; set; }

    [Parameter]
    public decimal StartingBalance { get; set; }

    [Parameter]
    public decimal StartingDate { get; set; }

    [Parameter]
    public EventCallback OnTransactionSaved { get; set; }

    private string MaxMonth => DateTime.Now.ToString("yyyy-MM");

    protected override void OnParametersSet()
    {
        _balance = StartingBalance;
        _transactionDate = DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1));
    }

    private void OnMonthChanged(ChangeEventArgs e)
    {
        if (DateTime.TryParse(e.Value + "-01", out var newDate))
        {
            // Restrict future month selection
            var maxAllowedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (newDate > maxAllowedDate)
            {
                newDate = maxAllowedDate;
            }

            _transactionDate = DateOnly.FromDateTime(newDate);
        }
    }

    private async Task SaveMonthlyBalance()
    {
        if (_transactionDate == null)
        {
            return;
        }

        await MonthlyBalanceService.AddMonthlyBalanceAsync(
            new MonthlyBalanceCreateDTO(AccountId, _transactionDate.Value, _balance));

        await OnTransactionSaved.InvokeAsync();
        await CloseModal();
    }

    private async Task CloseModal()
    {
        IsVisible = false;
        await IsVisibleChanged.InvokeAsync(false);
    }
}
