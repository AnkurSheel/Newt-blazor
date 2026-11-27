using Microsoft.AspNetCore.Components;

using NetWorthTracker.Business.Features.Accounts.Api;

namespace NetWorthTracker.UI.Feature.MonthlyBalance;

public partial class MonthlyCheckIn
{
    private IReadOnlyList<AccountResponseDTO> _eligibleAccounts = new List<AccountResponseDTO>();

    private bool _isTransactionModalOpen;

    private AccountResponseDTO? _selectedAccountForTransaction;

    [CascadingParameter]
    public DateOnly SelectedDate { get; set; }

    private IEnumerable<AccountResponseDTO> Accounts => _eligibleAccounts;

    private void OpenAddTransactionModal(AccountResponseDTO account)
    {
        if (account.BalanceOnDate.HasValue)
        {
            return;
        }

        _selectedAccountForTransaction = account;
        _isTransactionModalOpen = true;
    }

    private async Task Refresh()
    {
        await LoadAccountsAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadAccountsAsync();
    }

    private async Task LoadAccountsAsync()
    {
        _eligibleAccounts = await AccountService.GetActiveAccountsAsync(SelectedDate);
    }
}
