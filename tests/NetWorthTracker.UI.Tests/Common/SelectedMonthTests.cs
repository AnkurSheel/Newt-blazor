using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using NetWorthTracker.Business.Features.Accounts.Api;
using NetWorthTracker.Business.Features.FinancialSummary.Api;
using NetWorthTracker.Business.Features.MonthlyBalances.Api;
using NetWorthTracker.UI.Common.Layout;
using NetWorthTracker.UI.Feature.MonthlyBalance;
using NetWorthTracker.UI.Tests.TestHelpers;

namespace NetWorthTracker.UI.Tests.Common;

public sealed class SelectedMonthTests : BunitContext
{
    public SelectedMonthTests()
    {
        ServiceRegistry.ConfigureServices(Services);
        var clock = new Mock<TimeProvider>();
        clock.Setup(provider => provider.GetUtcNow()).Returns(new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero));
        clock.Setup(provider => provider.LocalTimeZone).Returns(TimeZoneInfo.Utc);
        Services.AddSingleton(clock.Object);
        var accounts = new Mock<IAccountService>();
        accounts.Setup(service => service.GetActiveAccountsAsync(It.IsAny<DateOnly>())).ReturnsAsync([]);
        Services.AddSingleton(accounts.Object);
        Services.AddSingleton(Mock.Of<IMonthlyBalanceService>());
        var summaries = new Mock<IFinancialSummaryService>();
        summaries.Setup(service => service.GetMonthlySummary(new DateOnly(2026, 6, 1)))
            .ReturnsAsync(new MonthlySummaryDTO(new DateOnly(2026, 6, 1), 1000m, 0m));
        summaries.Setup(service => service.GetMonthlySummary(new DateOnly(2026, 7, 1)))
            .ReturnsAsync(new MonthlySummaryDTO(new DateOnly(2026, 7, 1), 2000m, 0m));
        summaries.Setup(service => service.GetMonthlyNetworthChangeAsync(It.IsAny<DateOnly>()))
            .ReturnsAsync(MonthlyNetworthChangeDTO.Default);
        Services.AddSingleton(summaries.Object);
        SetRendererInfo(new RendererInfo("WebView", true));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task NextMonth_UpdatesFinancialSummaryOnMonthlyCheckIn()
    {
        RenderFragment page = builder =>
        {
            builder.OpenComponent<MonthlyCheckIn>(0);
            builder.CloseComponent();
        };
        var layout = Render<MainLayout>(parameters => parameters.Add(component => component.Body, page));

        await layout.GetByRole("button", "Next month").ClickAsync();

        await layout.WaitForAssertionAsync(() => Assert.Contains(2000m.ToString("C0"), layout.Markup));
    }
}
