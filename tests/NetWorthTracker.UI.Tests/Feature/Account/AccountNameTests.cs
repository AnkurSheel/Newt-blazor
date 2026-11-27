using Bunit;

using NetWorthTracker.TestHelpers;
using NetWorthTracker.UI.Feature.Account;

namespace NetWorthTracker.UI.Tests.Feature.Account;

public sealed class AccountNameTests : BunitContext
{
    [Fact]
    public void AccountNameDisplaysNameAndTitle()
    {
        var account = AccountTestHelpers.CreateAccountResponseDto(
            name: "Everyday account",
            openDate: new DateOnly(2026, 1, 1),
            closedDate: new DateOnly(2026, 6, 1));
        var selectedDate = new DateOnly(2026, 6, 1);

        var component = Render<AccountName>(parameters => parameters
            .Add(x => x.Account, account)
            .Add(x => x.SelectedDate, selectedDate));

        var name = component.Find("span");
        Assert.Multiple(
            () => Assert.Equal("Everyday account", name.TextContent.Trim()),
            () => Assert.Equal("Everyday account", name.GetAttribute("title")));
    }
}
