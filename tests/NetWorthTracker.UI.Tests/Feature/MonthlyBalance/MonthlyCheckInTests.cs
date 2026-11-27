using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using NetWorthTracker.Business.Features.Accounts.Api;
using NetWorthTracker.Business.Features.MonthlyBalances.Api;
using NetWorthTracker.TestHelpers;
using NetWorthTracker.UI.Components;
using NetWorthTracker.UI.Feature.MonthlyBalance;
using NetWorthTracker.UI.Tests.TestHelpers;

namespace NetWorthTracker.UI.Tests.Feature.MonthlyBalance;

public sealed class MonthlyCheckInTests : BunitContext
{
    private readonly Mock<IAccountService> _accountService = new();
    private readonly DateOnly _selectedDate = new(2026, 6, 1);

    public MonthlyCheckInTests()
    {
        ServiceRegistry.ConfigureServices(Services);
        Services.AddSingleton(_accountService.Object);
        Services.AddSingleton(Mock.Of<IMonthlyBalanceService>());
        ComponentFactories.AddStub<FinancialSummaryContainer>();
        SetRendererInfo(new RendererInfo("WebView", true));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task Active_account_names_are_displayed_on_initial_load()
    {
        AccountTestHelpers.SetupActiveAccountService(
            _accountService,
            new List<AccountResponseDTO>
            {
                AccountTestHelpers.CreateAccountResponseDto(name: "Checking")
            });

        IRenderedComponent<CascadingValue<DateOnly>> cut = RenderCheckIn(_selectedDate);

        await cut.WaitForAssertionAsync(() => Assert.Contains("Checking", cut.Markup));
    }

    [Fact]
    public async Task Balance_is_formatted_and_add_action_disabled_when_balance_exists()
    {
        AccountTestHelpers.SetupActiveAccountService(
            _accountService,
            new List<AccountResponseDTO>
            {
                AccountTestHelpers.CreateAccountResponseDto(name: "Checking", balanceOnDate: 1234.5m)
            });

        IRenderedComponent<CascadingValue<DateOnly>> cut = RenderCheckIn(_selectedDate);

        await cut.WaitForAssertionAsync(() =>
        {
            Assert.Multiple(
                () => Assert.Contains(1234.5m.ToString("C2"), cut.Markup),
                () => Assert.True(
                    cut.GetByRole("button", "+ Txn")
                        .HasAttribute("disabled")));
        });
    }

    [Fact]
    public async Task Add_action_opens_modal_when_balance_is_missing()
    {
        AccountTestHelpers.SetupActiveAccountService(
            _accountService,
            new List<AccountResponseDTO>
            {
                AccountTestHelpers.CreateAccountResponseDto(name: "Checking")
            });

        IRenderedComponent<CascadingValue<DateOnly>> cut = RenderCheckIn(_selectedDate);

        await cut.GetByRole("button", "+ Txn").ClickAsync();

        await cut.WaitForAssertionAsync(() => Assert.Contains("Save Transaction", cut.Markup));
    }

    [Fact]
    public async Task Add_action_is_enabled_when_balance_is_missing()
    {
        AccountTestHelpers.SetupActiveAccountService(
            _accountService,
            new List<AccountResponseDTO>
            {
                AccountTestHelpers.CreateAccountResponseDto(name: "Checking")
            });

        IRenderedComponent<CascadingValue<DateOnly>> cut = RenderCheckIn(_selectedDate);

        await cut.WaitForAssertionAsync(() => Assert.False(
            cut.GetByRole("button", "+ Txn")
                .HasAttribute("disabled")));
    }

    [Fact]
    public async Task Zero_balance_is_displayed_and_add_action_disabled()
    {
        AccountTestHelpers.SetupActiveAccountService(
            _accountService,
            new List<AccountResponseDTO>
            {
                AccountTestHelpers.CreateAccountResponseDto(name: "Checking", balanceOnDate: 0m)
            });

        IRenderedComponent<CascadingValue<DateOnly>> cut = RenderCheckIn(_selectedDate);

        await cut.WaitForAssertionAsync(() =>
        {
            Assert.Multiple(
                () => Assert.Contains(0m.ToString("C2"), cut.Markup),
                () => Assert.True(
                    cut.GetByRole("button", "+ Txn")
                        .HasAttribute("disabled")));
        });
    }

    [Fact]
    public async Task Empty_state_and_accounts_link_are_shown_when_no_active_accounts_exist()
    {
        AccountTestHelpers.SetupActiveAccountService(_accountService, new List<AccountResponseDTO>());

        IRenderedComponent<CascadingValue<DateOnly>> cut = RenderCheckIn(_selectedDate);

        await cut.WaitForAssertionAsync(() =>
        {
            Assert.Multiple(
                () => Assert.Contains("There are no active accounts", cut.Markup),
                () => Assert.Contains("href=\"/accounts\"", cut.Markup),
                () => Assert.Empty(cut.FindAll("table")));
        });
    }

    private IRenderedComponent<CascadingValue<DateOnly>> RenderCheckIn(DateOnly selectedDate)
    {
        return Render<CascadingValue<DateOnly>>(parameters => parameters.Add(component => component.Value, selectedDate)
            .AddChildContent<MonthlyCheckIn>());
    }
}
