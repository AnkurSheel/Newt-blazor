using NetWorthTracker.Core.Features.Account;

namespace NetWorthTracker.UI.Feature.Account;

public partial class AccountList
{
    private IReadOnlyList<AccountResponseDTO> _accounts = new List<AccountResponseDTO>();
    private string _filterStatus = "Active";
    private bool _isOpen;

    private bool _isTransactionModalOpen;

    private AccountResponseDTO? _selectedAccountForTransaction;

    private decimal TotalAssets => _accounts.Where(a => !a.IsClosed && a.Type == AccountType.ASSET)
        .Sum(a => a.LatestBalance);

    private decimal TotalLiabilities => _accounts.Where(a => !a.IsClosed && a.Type == AccountType.LIABILITY)
        .Sum(a => a.LatestBalance);

    private decimal NetWorth => TotalAssets - TotalLiabilities;

    private IEnumerable<AccountResponseDTO> FilteredAccounts => _filterStatus switch
    {
        "Active" => _accounts.Where(a => !a.IsClosed),
        "Closed" => _accounts.Where(a => a.IsClosed),
        _ => _accounts
    };

    protected override async Task OnInitializedAsync()
    {
        _accounts = await AccountService.GetAccountsAsync();
    }

    private void OpenAddTransactionModal(AccountResponseDTO account)
    {
        if (account.IsClosed)
        {
            return;
        }

        _selectedAccountForTransaction = account;
        _isTransactionModalOpen = true;
    }

    private async Task Refresh()
    {
        _accounts = await AccountService.GetAccountsAsync();
    }
}
