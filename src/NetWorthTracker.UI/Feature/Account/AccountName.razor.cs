using Microsoft.AspNetCore.Components;

using NetWorthTracker.Business.Features.Accounts.Api;

namespace NetWorthTracker.UI.Feature.Account;

public partial class AccountName
{
    [Parameter]
    public required AccountResponseDTO Account { get; set; }

    [Parameter]
    public DateOnly SelectedDate { get; set; }
}
