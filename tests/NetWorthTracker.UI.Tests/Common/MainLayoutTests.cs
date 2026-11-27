using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using NetWorthTracker.Business.Features.Accounts.Api;
using NetWorthTracker.Business.Features.FinancialSummary.Api;
using NetWorthTracker.Business.Features.MonthlyBalances.Api;
using NetWorthTracker.TestHelpers;
using NetWorthTracker.UI.Common.Layout;
using NetWorthTracker.UI.Components;
using NetWorthTracker.UI.Feature.Account;
using NetWorthTracker.UI.Feature.Dashboard;
using NetWorthTracker.UI.Feature.MonthlyBalance;
using NetWorthTracker.UI.Tests.TestHelpers;

namespace NetWorthTracker.UI.Tests.Common;

public sealed class MainLayoutTests : BunitContext
{
    private readonly Mock<IAccountService> _accounts = new();

    public MainLayoutTests()
    {
        ServiceRegistry.ConfigureServices(Services);
        var clock = new Mock<TimeProvider>();
        clock.Setup(provider => provider.GetUtcNow()).Returns(new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero));
        clock.Setup(provider => provider.LocalTimeZone).Returns(TimeZoneInfo.Utc);
        Services.AddSingleton(clock.Object);
        _accounts.Setup(service => service.GetActiveAccountsAsync(It.IsAny<DateOnly>())).ReturnsAsync([]);
        Services.AddSingleton(_accounts.Object);
        Services.AddSingleton(Mock.Of<IMonthlyBalanceService>());
        var summaries = new Mock<IFinancialSummaryService>();
        summaries.Setup(service => service.GetMonthlyTrendsAsync(It.IsAny<DateOnly>())).ReturnsAsync([]);
        summaries.Setup(service => service.GetYearlyTrendsAsync(It.IsAny<DateOnly>())).ReturnsAsync([]);
        Services.AddSingleton(summaries.Object);
        ComponentFactories.AddStub<FinancialSummaryContainer>("<section>Financial summary</section>");
        ComponentFactories.AddStub<Trends>();
        SetRendererInfo(new RendererInfo("WebView", true));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Account_screen_hides_month_controls_and_financial_summary()
    {
        var layout = Render<MainLayout>(parameters => parameters.Add(component => component.Body, Screen<AccountList>()));

        Assert.Multiple(
            () => Assert.Empty(layout.FindAll("button[aria-label='Previous month']")),
            () => Assert.DoesNotContain("Financial summary", layout.Markup));
    }

    [Fact]
    public void Dashboard_shows_month_controls_and_financial_summary()
    {
        var layout = Render<MainLayout>(parameters => parameters.Add(component => component.Body, Screen<Dashboard>()));

        Assert.Multiple(
            () => Assert.NotEmpty(layout.FindAll("button[aria-label='Previous month']")),
            () => Assert.Contains("Financial summary", layout.Markup));
    }

    [Fact]
    public void Monthly_check_in_shows_month_controls_and_financial_summary()
    {
        var layout = Render<MainLayout>(parameters => parameters.Add(component => component.Body, Screen<MonthlyCheckIn>()));

        Assert.Multiple(
            () => Assert.NotEmpty(layout.FindAll("button[aria-label='Previous month']")),
            () => Assert.Contains("Financial summary", layout.Markup));
    }

    [Fact]
    public async Task Selected_month_is_preserved_when_returning_to_monthly_check_in()
    {
        // Arrange a selected month and navigate away from the check-in screen.
        _accounts.Setup(service => service.GetActiveAccountsAsync(new DateOnly(2026, 7, 1)))
            .ReturnsAsync([AccountTestHelpers.CreateAccountResponseDto(name: "July account")]);
        var layout = Render<MainLayout>(parameters => parameters.Add(component => component.Body, Screen<MonthlyCheckIn>()));
        await layout.GetByRole("button", "Next month").ClickAsync();
        layout.Render(parameters => parameters.Add(component => component.Body, Screen<AccountList>()));

        // Act
        layout.Render(parameters => parameters.Add(component => component.Body, Screen<MonthlyCheckIn>()));

        // Assert
        await layout.WaitForAssertionAsync(() => Assert.Contains("July account", layout.Markup));
    }

    private static RenderFragment Screen<T>() where T : IComponent
    {
        return builder =>
        {
            builder.OpenComponent<T>(0);
            builder.CloseComponent();
        };
    }
}
