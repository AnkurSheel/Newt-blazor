using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using NetWorthTracker.Business.Features.Accounts.Api;
using NetWorthTracker.Business.Features.MonthlyBalances.Api;
using NetWorthTracker.TestHelpers;
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
        SetRendererInfo(new RendererInfo("WebView", true));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task InitialLoad_WhenActiveAccountsExist_DisplaysTheirNames()
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
    public async Task WhenBalanceExists_DisplaysFormattedBalanceAndDisablesAddTransaction()
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
            Assert.Contains(1234.5m.ToString("C2"), cut.Markup);
            Assert.True(
                cut.GetByRole("button", "+ Txn")
                    .HasAttribute("disabled"));
        });
    }

        [Fact]
        public async Task WhenBalanceIsMissing_AddActionOpensModal()
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
    public async Task WhenBalanceIsMissing_DisplaysEnabledAddTransaction()
    {
        AccountTestHelpers.SetupActiveAccountService(
            _accountService,
            new List<AccountResponseDTO>
            {
                AccountTestHelpers.CreateAccountResponseDto(name: "Checking")
            });

        IRenderedComponent<CascadingValue<DateOnly>> cut = RenderCheckIn(_selectedDate);

        await cut.WaitForAssertionAsync(() =>
        {
            Assert.Contains("Checking", cut.Markup);
            Assert.False(
                cut.GetByRole("button", "+ Txn")
                    .HasAttribute("disabled"));
        });
    }

    [Fact]
    public async Task WhenBalanceIsZero_DisplaysZeroAndDisablesAddTransaction()
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
            Assert.Contains(0m.ToString("C2"), cut.Markup);
            Assert.True(
                cut.GetByRole("button", "+ Txn")
                    .HasAttribute("disabled"));
        });
    }

    [Fact]
    public async Task WhenNoActiveAccountsExist_ShowsEmptyStateAndAccountsLink()
    {
        AccountTestHelpers.SetupActiveAccountService(_accountService, new List<AccountResponseDTO>());

        IRenderedComponent<CascadingValue<DateOnly>> cut = RenderCheckIn(_selectedDate);

        await cut.WaitForAssertionAsync(() =>
        {
            Assert.Contains("There are no active accounts", cut.Markup);
            Assert.Contains("href=\"/accounts\"", cut.Markup);
            Assert.Empty(cut.FindAll("table"));
        });
    }

    private IRenderedComponent<CascadingValue<DateOnly>> RenderCheckIn(DateOnly selectedDate)
    {
        return Render<CascadingValue<DateOnly>>(parameters => parameters.Add(component => component.Value, selectedDate)
            .AddChildContent<MonthlyCheckIn>());
    }
}
