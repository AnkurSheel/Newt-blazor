using Microsoft.AspNetCore.Components;

using NetWorthTracker.Core.Features.Account;

namespace NetWorthTracker.UI.Feature.Account;

public partial class AddAccountModal
{
    [Parameter] 
    public bool IsVisible { get; set; }

    [Parameter]
    public EventCallback<bool> IsVisibleChanged { get; set; }

    private AccountType _newAccountType;
    private string _newAccountName = string.Empty;
    private bool _isAccountClosed;
    private DateOnly? _closedDate;

    private async Task OnVisibleChanged(bool value)
    {
        IsVisible = value;
        await IsVisibleChanged.InvokeAsync(value);
    }

    private async Task CloseModal()
    {
        await OnVisibleChanged(false);
        ResetForm();
    }

    private void ToggleClosedState(ChangeEventArgs e)
    {
        _isAccountClosed = (bool)(e.Value ?? false);
        if (!_isAccountClosed)
        {
            _closedDate = null;
        }
    }

    private async Task SaveAccount()
    {
        await AccountService.AddAccountAsync(_newAccountName, _newAccountType, _closedDate);
        await CloseModal();
    }

    private void ResetForm()
    {
        _newAccountName = string.Empty;
        _newAccountType = AccountType.ASSET;
        _isAccountClosed = false;
        _closedDate = null;
    }
}
