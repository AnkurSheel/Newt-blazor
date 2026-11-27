using Microsoft.AspNetCore.Components;

using NetWorthTracker.Business.Features.Accounts.Api;

namespace NetWorthTracker.UI.Feature.Account;

public partial class AccountList
{
    private IReadOnlyList<AccountResponseDTO> _accounts = new List<AccountResponseDTO>();
    private string _filterStatus = "Active";
    private bool _isOpen;
    private DateOnly _currentMonth;

    [Inject]
    private TimeProvider Clock { get; set; } = null!;

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

    protected override async Task OnInitializedAsync()
    {
        await LoadAccountsAsync();
    }

    private async Task LoadAccountsAsync()
    {
        var localNow = Clock.GetLocalNow();
        _currentMonth = new DateOnly(localNow.Year, localNow.Month, 1);
        _accounts = _filterStatus switch
        {
            "Active" => await AccountService.GetActiveAccountsAsync(_currentMonth),
            "Closed" => await AccountService.GetClosedAccountsAsync(_currentMonth),
            _ => await AccountService.GetAccountsAsync(_currentMonth)
        };
    }
}
