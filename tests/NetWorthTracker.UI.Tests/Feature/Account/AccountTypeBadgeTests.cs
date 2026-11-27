using Bunit;

using NetWorthTracker.Business.Features.Accounts.Api;
using NetWorthTracker.UI.Feature.Account;

namespace NetWorthTracker.UI.Tests.Feature.Account;

public sealed class AccountTypeBadgeTests : BunitContext
{
    [Theory]
    [InlineData(AccountType.ASSET, "Asset")]
    [InlineData(AccountType.LIABILITY, "Liability")]
    public void Render_DisplaysTheLabelForProvidedAccountType(AccountType type, string label)
    {
        var component = Render<AccountTypeBadge>(parameters => parameters.Add(x => x.Type, type));

        var badge = component.Find("span");
        Assert.Equal(label, badge.TextContent.Trim());
    }
}
