using Microsoft.AspNetCore.Components;

using NetWorthTracker.Business.Features.Accounts.Api;

namespace NetWorthTracker.UI.Feature.Account;

public partial class AccountList
{
    private IReadOnlyList<AccountResponseDTO> _accounts = new List<AccountResponseDTO>();
    private string _filterStatus = "Active";
    private bool _isOpen;

    [CascadingParameter]
    public DateOnly SelectedDate { get; set; }

    [Inject]
    private IAccountService AccountService { get; set; } = null!;

    private async Task Refresh()
    {
        await LoadAccountsAsync();
    }

    private async Task SelectFilterAsync(string filterStatus)
    {
        _filterStatus = filterStatus;
        await LoadAccountsAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadAccountsAsync();
    }

    private async Task LoadAccountsAsync()
    {
        _accounts = _filterStatus switch
        {
            "Active" => await AccountService.GetActiveAccountsAsync(SelectedDate),
            "Closed" => await AccountService.GetClosedAccountsAsync(SelectedDate),
            _ => await AccountService.GetAccountsAsync(SelectedDate)
        };
    }
}
