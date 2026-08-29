using NetWorthTracker.Core.Features.Account;

namespace NetWorthTracker.UI.Feature.Account;

public partial class AccountList
{
    private IReadOnlyList<AccountResponseDTO> _accounts = new List<AccountResponseDTO>();
    private string _filterStatus = "Active";
    private bool _isOpen;

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
}
