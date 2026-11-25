using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using NetWorthTracker.Business.Features.Accounts.Api;
using NetWorthTracker.TestHelpers;
using NetWorthTracker.UI.Feature.Account;
using NetWorthTracker.UI.Tests.TestHelpers;

namespace NetWorthTracker.UI.Tests.Feature.Account;

public sealed class AccountListTests : BunitContext
{
    private readonly Mock<IAccountService> _accountService = new();
    private readonly DateOnly _selectedDate = new(2026, 6, 1);

    public AccountListTests()
    {
        ServiceRegistry.ConfigureServices(Services);
        Services.AddSingleton(_accountService.Object);
        SetRendererInfo(new RendererInfo("WebView", true));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task InitialLoad_DisplaysActiveAccountsForSelectedDate()
    {
        AccountTestHelpers.SetupActiveAccountService(
            _accountService,
            new List<AccountResponseDTO> { AccountTestHelpers.CreateAccountResponseDto(name: "Active Account") });

        IRenderedComponent<CascadingValue<DateOnly>> cut = RenderList(_selectedDate);

        await cut.WaitForAssertionAsync(() => Assert.Contains("Active Account", cut.Markup));
    }

    [Fact]
    public async Task ClosedFilter_DisplaysClosedAccounts()
    {
        AccountTestHelpers.SetupActiveAccountService(_accountService, []);

        AccountTestHelpers.SetupClosedAccountService(
            _accountService,
            new List<AccountResponseDTO>
            {
                AccountTestHelpers.CreateAccountResponseDto(name: "Closed Account", closedDate: _selectedDate)
            });

        IRenderedComponent<CascadingValue<DateOnly>> cut = RenderList(_selectedDate);
        await cut.GetByRole("button", "Closed")
            .ClickAsync();

        await cut.WaitForAssertionAsync(() => Assert.Contains("Closed Account", cut.Markup));
    }

    [Fact]
    public async Task AllFilter_DisplaysAccountsThatAreNotYetOpen()
    {
        AccountTestHelpers.SetupActiveAccountService(
            _accountService,
            new List<AccountResponseDTO>
            {
                AccountTestHelpers.CreateAccountResponseDto(name: "Active Account"),
                AccountTestHelpers.CreateAccountResponseDto(name: "Future Account", openDate: new DateOnly(2026, 7, 1))
            });
        _accountService.Setup(service => service.GetAccountsAsync(It.IsAny<DateOnly>()))
            .ReturnsAsync(
            [
                AccountTestHelpers.CreateAccountResponseDto(name: "Active Account"),
                AccountTestHelpers.CreateAccountResponseDto(name: "Future Account", openDate: new DateOnly(2026, 7, 1))
            ]);

        IRenderedComponent<CascadingValue<DateOnly>> cut = RenderList(_selectedDate);
        await cut.GetByRole("button", "All")
            .ClickAsync();

        await cut.WaitForAssertionAsync(() =>
        {
            Assert.Contains("Active Account", cut.Markup);
            Assert.Contains("Future Account", cut.Markup);
            Assert.Contains("Not yet open", cut.Markup);
        });
    }

    [Fact]
    public async Task SelectedDateChanges_ReloadsActiveAccountsForNewDate()
    {
        var firstDate = new DateOnly(2026, 5, 1);
        var secondDate = new DateOnly(2026, 6, 1);

        _accountService.Setup(service => service.GetActiveAccountsAsync(firstDate))
            .ReturnsAsync(
            [
                AccountTestHelpers.CreateAccountResponseDto(name: "May Active", openDate: new DateOnly(2026, 1, 1))
            ]);
        _accountService.Setup(service => service.GetActiveAccountsAsync(secondDate))
            .ReturnsAsync(
            [
                AccountTestHelpers.CreateAccountResponseDto(name: "June Active", openDate: new DateOnly(2026, 1, 1))
            ]);

        IRenderedComponent<CascadingValue<DateOnly>> cut = RenderList(firstDate);
        await cut.WaitForAssertionAsync(() => Assert.Contains("May Active", cut.Markup));

        cut.Render(parameters => parameters.Add(component => component.Value, secondDate)
            .AddChildContent<AccountList>());

        await cut.WaitForAssertionAsync(() =>
        {
            Assert.Contains("June Active", cut.Markup);
            Assert.DoesNotContain("May Active", cut.Markup);
        });
    }

    private IRenderedComponent<CascadingValue<DateOnly>> RenderList(DateOnly selectedDate)
    {
        return Render<CascadingValue<DateOnly>>(parameters => parameters.Add(component => component.Value, selectedDate)
            .AddChildContent<AccountList>());
    }
}
