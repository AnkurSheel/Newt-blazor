using Microsoft.AspNetCore.Components;

using NetWorthTracker.Business.Features.MonthlyBalances.Api;

namespace NetWorthTracker.UI.Feature.MonthlyBalance;

public partial class AddMonthlyBalanceModal
{
    private decimal _balance;
    private DateOnly? _transactionDate;
    private string? _errorMessage;
    private bool _isSaving;

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
    public EventCallback OnTransactionSaved { get; set; }

    private string MaxMonth => DateTime.Now.ToString("yyyy-MM");

    protected override void OnParametersSet()
    {
        _balance = StartingBalance;
        _transactionDate = DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));
    }

    private async Task CopyBalanceFromLastMonthAsync()
    {
        if (_transactionDate == null)
        {
            return;
        }

        var lastMonthDate = _transactionDate.Value.AddMonths(-1);
        var lastMonthBalance = await MonthlyBalanceService.GetBalanceByDateAsync(AccountId, lastMonthDate);
        _balance = lastMonthBalance ?? 0.0m;
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

    private void ClearError()
    {
        _errorMessage = null;
    }

    private async Task SaveMonthlyBalance()
    {
        if (_isSaving)
        {
            return;
        }

        if (_transactionDate == null)
        {
            return;
        }

        _isSaving = true;
        _errorMessage = null;
        try
        {
            var result = await MonthlyBalanceService.AddMonthlyBalanceAsync(
                new MonthlyBalanceCreateDTO(AccountId, _transactionDate.Value, _balance));
            if (result.IsFailure)
            {
                _errorMessage = result.Error.Message;
                return;
            }

            await OnTransactionSaved.InvokeAsync();
            await CloseModal();
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task CloseModal()
    {
        _errorMessage = null;
        IsVisible = false;
        await IsVisibleChanged.InvokeAsync(false);
    }
}
