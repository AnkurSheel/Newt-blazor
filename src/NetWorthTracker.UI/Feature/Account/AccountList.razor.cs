using NetWorthTracker.Core.Features.Account;

namespace NetWorthTracker.UI.Feature.Account;

public partial class AccountList
{
    private IReadOnlyList<AccountResponseDTO> _accounts = new List<AccountResponseDTO>();
    private string _filterStatus = "Active";
    private bool _isOpen;

    private bool _isTransactionModalOpen;

    private AccountResponseDTO? _selectedAccountForTransaction;
    private DateOnly _selectedDate = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);

    private decimal TotalAssets => _accounts.Where(a => !a.IsClosedOn(_selectedDate) && a.Type == AccountType.ASSET)
        .Sum(a => a.LatestBalance);

    private decimal TotalLiabilities => _accounts
        .Where(a => !a.IsClosedOn(_selectedDate) && a.Type == AccountType.LIABILITY)
        .Sum(a => a.LatestBalance);

    private decimal NetWorth => TotalAssets - TotalLiabilities;

    private IEnumerable<AccountResponseDTO> FilteredAccounts => _filterStatus switch
    {
        "Active" => _accounts.Where(a => !a.IsClosedOn(_selectedDate)),
        "Closed" => _accounts.Where(a => a.IsClosedOn(_selectedDate)),
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
        _accounts = await AccountService.GetAccountsAsync(_selectedDate);
    }

    private async Task OnDateChanged(DateOnly newDate)
    {
        _selectedDate = newDate;
        await LoadAccountsAsync();
    }

    private async Task LoadAccountsAsync()
    {
        _accounts = await AccountService.GetAccountsAsync(_selectedDate);
    }
}
