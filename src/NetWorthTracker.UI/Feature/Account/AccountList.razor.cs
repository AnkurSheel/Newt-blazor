using Microsoft.AspNetCore.Components;

using NetWorthTracker.Core.Features.Account;

namespace NetWorthTracker.UI.Feature.Account;

public partial class AccountList
{
    private IReadOnlyList<AccountResponseDTO> _accounts = new List<AccountResponseDTO>();
    private string _filterStatus = "Active";
    private bool _isOpen;

    private bool _isTransactionModalOpen;

    private AccountResponseDTO? _selectedAccountForTransaction;

    [CascadingParameter]
    public DateOnly SelectedDate { get; set; }

    private IEnumerable<AccountResponseDTO> FilteredAccounts => _filterStatus switch
    {
        "Active" => _accounts.Where(a => !a.IsClosedOn(SelectedDate)),
        "Closed" => _accounts.Where(a => a.IsClosedOn(SelectedDate)),
        _ => _accounts
    };

    protected override async Task OnInitializedAsync()
    {
        await LoadAccountsAsync();
    }

    private void OpenAddTransactionModal(AccountResponseDTO account)
    {
        if (account.IsClosedOn(DateOnly.FromDateTime(DateTime.Now)))
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
        _accounts = await AccountService.GetAccountsAsync(SelectedDate);
    }
}
