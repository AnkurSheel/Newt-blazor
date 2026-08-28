using NetWorthTracker.Core.Features.Account;

namespace NetWorthTracker.UI.Feature.Account;

public partial class AccountList
{
    private IReadOnlyList<AccountDTO> _accounts = new List<AccountDTO>();
    private string _filterStatus = "Active";
    private bool _isOpen;

    private IEnumerable<AccountDTO> FilteredAccounts => _filterStatus switch
    {
        "Active" => _accounts.Where(a => !a.IsClosed),
        "Closed" => _accounts.Where(a => a.IsClosed),
        _ => _accounts
    };

    protected override async Task OnInitializedAsync()
    {
        _accounts = await AccountService.GetAccountsAsync();
    }
}
