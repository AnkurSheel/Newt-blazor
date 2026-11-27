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
    private readonly Mock<TimeProvider> _clock = new();
    private readonly DateOnly _selectedDate = new(2026, 6, 1);

    public AccountListTests()
    {
        ServiceRegistry.ConfigureServices(Services);
        Services.AddSingleton(_accountService.Object);
        _clock.Setup(provider => provider.GetUtcNow()).Returns(new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero));
        _clock.Setup(provider => provider.LocalTimeZone).Returns(TimeZoneInfo.Utc);
        Services.AddSingleton(_clock.Object);
        SetRendererInfo(new RendererInfo("WebView", true));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task Local_month_is_used_when_it_differs_from_utc_on_initial_load()
    {
        _clock.Setup(provider => provider.GetUtcNow()).Returns(new DateTimeOffset(2026, 5, 31, 23, 0, 0, TimeSpan.Zero));
        _clock.Setup(provider => provider.LocalTimeZone)
            .Returns(TimeZoneInfo.CreateCustomTimeZone("Test", TimeSpan.FromHours(2), "Test", "Test"));
        _accountService.Setup(service => service.GetActiveAccountsAsync(new DateOnly(2026, 6, 1)))
            .ReturnsAsync([AccountTestHelpers.CreateAccountResponseDto(name: "June account")]);

        var cut = RenderList(new DateOnly(2026, 5, 1));

        await cut.WaitForAssertionAsync(() => Assert.Contains("June account", cut.Markup));
    }

    [Fact]
    public async Task Active_accounts_are_displayed_for_current_month()
    {
        AccountTestHelpers.SetupActiveAccountService(
            _accountService,
            new List<AccountResponseDTO> { AccountTestHelpers.CreateAccountResponseDto(name: "Active Account") });

        IRenderedComponent<CascadingValue<DateOnly>> cut = RenderList(_selectedDate);

        await cut.WaitForAssertionAsync(() => Assert.Contains("Active Account", cut.Markup));
    }

    [Fact]
    public async Task Closed_accounts_are_displayed_when_closed_filter_is_selected()
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
    public async Task Future_accounts_are_displayed_when_all_filter_is_selected()
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
            Assert.Multiple(
                () => Assert.Contains("Active Account", cut.Markup),
                () => Assert.Contains("Future Account", cut.Markup),
                () => Assert.Contains("Not yet open", cut.Markup));
        });
    }

    [Fact]
    public async Task Account_statuses_use_current_local_month_when_all_filter_is_selected()
    {
        AccountTestHelpers.SetupActiveAccountService(_accountService, []);
        _accountService.Setup(service => service.GetAccountsAsync(new DateOnly(2026, 6, 1)))
            .ReturnsAsync(
            [
                AccountTestHelpers.CreateAccountResponseDto(1, "Open", new DateOnly(2026, 6, 1)),
                AccountTestHelpers.CreateAccountResponseDto(
                    2, "Closed", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 1)),
                AccountTestHelpers.CreateAccountResponseDto(3, "Future", new DateOnly(2026, 7, 1))
            ]);
        IRenderedComponent<CascadingValue<DateOnly>> cut = RenderList(new DateOnly(2026, 1, 1));

        await cut.GetByRole("button", "All").ClickAsync();

        await cut.WaitForAssertionAsync(() =>
        {
            Assert.Multiple(
                () => Assert.Contains("Closed Jun, 2026", cut.Markup),
                () => Assert.Contains("Not yet open", cut.Markup),
                () => Assert.Contains("Active", cut.Find("span[title='Open']").Closest("tr")!.TextContent));
        });
    }

    private IRenderedComponent<CascadingValue<DateOnly>> RenderList(DateOnly selectedDate)
    {
        return Render<CascadingValue<DateOnly>>(parameters => parameters.Add(component => component.Value, selectedDate)
            .AddChildContent<AccountList>());
    }
}
