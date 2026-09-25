using NetWorthTracker.Core.Features.Account;
using NetWorthTracker.Core.Features.FinancialSummary;

namespace NetWorthTracker.UI.Feature.Account;

public partial class AccountList
{
    private IReadOnlyList<AccountResponseDTO> _accounts = new List<AccountResponseDTO>();
    private string _filterStatus = "Active";
    private FinancialSummaryDTO _financialSummary = new(0, 0, 0);
    private bool _isOpen;

    private bool _isTransactionModalOpen;

    private AccountResponseDTO? _selectedAccountForTransaction;
    private DateOnly _selectedDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);

    private IEnumerable<AccountResponseDTO> FilteredAccounts => _filterStatus switch
    {
        "Active" => _accounts.Where(a => !a.IsClosedOn(_selectedDate)),
        "Closed" => _accounts.Where(a => a.IsClosedOn(_selectedDate)),
        _ => _accounts
    };

    protected override async Task OnInitializedAsync()
    {
        await LoadAccountsAsync();
        await LoadFinancialSummary();
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
        _financialSummary = await FinancialSummaryService.GetFinancialSummaryAsync(_selectedDate);
    }

    private async Task OnDateChanged(DateOnly newDate)
    {
        _selectedDate = newDate;
        await LoadAccountsAsync();
        await LoadFinancialSummary();
    }

    private async Task LoadAccountsAsync()
    {
        _accounts = await AccountService.GetAccountsAsync(_selectedDate);
    }

    private async Task LoadFinancialSummary()
    {
        _financialSummary = await FinancialSummaryService.GetFinancialSummaryAsync(_selectedDate);
    }
}
