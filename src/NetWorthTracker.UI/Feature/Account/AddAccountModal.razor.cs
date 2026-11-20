using Microsoft.AspNetCore.Components;

using NetWorthTracker.Business.Features.Accounts.Api;

namespace NetWorthTracker.UI.Feature.Account;

public partial class AddAccountModal
{
    [Parameter] 
    public bool IsVisible { get; set; }

    [Parameter]
    public EventCallback<bool> IsVisibleChanged { get; set; }

    [Parameter]
    public EventCallback OnAccountSaved { get; set; }


    private AccountType _newAccountType;
    private string _newAccountName = string.Empty;
    private bool _isAccountClosed;
    private DateOnly? _closedDate;
    private DateOnly? _openDate = DateOnly.FromDateTime(DateTime.Now);
    private string? _accountNameError;
    private bool _isSaving;

    private async Task OnVisibleChanged(bool value)
    {
        if (!value)
        {
            ClearAccountNameError();
        }

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
        if (_isSaving)
        {
            return;
        }

        _isSaving = true;
        ClearAccountNameError();
        var submittedName = _newAccountName;
        try
        {
            if (_openDate is not null)
            {
                try
                {
                    await AccountService.AddAccountAsync(
                        new AccountCreateDTO(submittedName, _newAccountType, _openDate.Value, _closedDate));
                }
                catch (DuplicateAccountException ex)
                {
                    if (IsVisible && _newAccountName == submittedName)
                    {
                        _accountNameError = ex.Message;
                    }

                    return;
                }
            }

            await OnAccountSaved.InvokeAsync();
            await CloseModal();
        }
        finally
        {
            _isSaving = false;
        }
    }

    private void ClearAccountNameError()
    {
        _accountNameError = null;
    }

    private void ResetForm()
    {
        _newAccountName = string.Empty;
        _newAccountType = AccountType.ASSET;
        _isAccountClosed = false;
        _closedDate = null;
    }
}
