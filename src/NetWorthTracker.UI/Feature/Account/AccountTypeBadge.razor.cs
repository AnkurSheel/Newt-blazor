using Microsoft.AspNetCore.Components;

using NetWorthTracker.Business.Features.Accounts.Api;

namespace NetWorthTracker.UI.Feature.Account;

public partial class AccountTypeBadge
{
    [Parameter]
    public AccountType Type { get; set; }
}
