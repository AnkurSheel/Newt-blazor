using Microsoft.AspNetCore.Components;

using NetWorthTracker.Core.Exceptions;
using NetWorthTracker.Core.Features.Account;

namespace NetWorthTracker.UI.Feature.Account;

public partial class AccountForm
{
    private readonly FormModel _model = new();
    private string? _error;

    [Parameter]
    public EventCallback OnSaved { get; set; }

    private IReadOnlyList<string> EnumNames
        => Enum.GetNames<AccountType>().Where(name => name != nameof(AccountType.DONOTUSE)).ToList();

    private async Task SubmitAsync()
    {
        try
        {
            await _accountService.AddAccountAsync(_model.Name, AccountType.ASSET, _model.ClosedDate);
            _model.Name = string.Empty;
            _error = null;
            await OnSaved.InvokeAsync();
        }
        catch (ValidationException ex)
        {
            _error = ex.Message;
        }
    }

    private class FormModel
    {
        public string Name { get; set; } = string.Empty;

        public AccountType Type { get; set; }

        public DateOnly? ClosedDate { get; set; }
    }
}
