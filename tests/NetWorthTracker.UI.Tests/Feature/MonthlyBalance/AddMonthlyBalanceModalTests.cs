using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using NetWorthTracker.Business.Features.MonthlyBalances.Api;
using NetWorthTracker.Business.Results;
using NetWorthTracker.UI.Components;
using NetWorthTracker.UI.Feature.MonthlyBalance;
using NetWorthTracker.UI.Tests.TestHelpers;

namespace NetWorthTracker.UI.Tests.Feature.MonthlyBalance;

public sealed class AddMonthlyBalanceModalTests : BunitContext
{
    private readonly Mock<IMonthlyBalanceService> _monthlyBalanceService = new();

    public AddMonthlyBalanceModalTests()
    {
        ServiceRegistry.ConfigureServices(Services);
        Services.AddSingleton(_monthlyBalanceService.Object);
        SetRendererInfo(new RendererInfo("WebView", true));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task Eligibility_error_is_shown_and_modal_stays_open_when_account_is_not_eligible()
    {
        SetupMonthlyBalanceServiceError(MonthlyBalanceErrors.AccountNotEligible);

        var saved = false;
        var closed = false;
        IRenderedComponent<AddMonthlyBalanceModal> modal = RenderModal(
            () => saved = true,
            visible => closed = !visible);

        await PressSaveButton(modal);

        Assert.Multiple(
            () => Assert.Equal(
                MonthlyBalanceErrors.AccountNotEligible.Message,
                modal.GetByRole("alert").TextContent),
            () => Assert.False(saved),
            () => Assert.False(closed));
    }

    [Fact]
    public async Task Successful_save_invokes_callback_and_closes_modal()
    {
        SetupMonthlyBalanceServiceSuccess();

        var saved = false;
        var closed = false;
        IRenderedComponent<AddMonthlyBalanceModal> modal = RenderModal(
            () => saved = true,
            visible => closed = !visible);

        await PressSaveButton(modal);

        Assert.Multiple(
            () => Assert.True(saved),
            () => Assert.True(closed));
    }

    [Fact]
    public async Task Eligibility_error_is_cleared_when_month_changes()
    {
        // Arrange the modal with an eligibility error.
        SetupMonthlyBalanceServiceError(MonthlyBalanceErrors.AccountNotEligible);
        IRenderedComponent<AddMonthlyBalanceModal> modal = RenderModal();
        await PressSaveButton(modal);

        // Act
        await ChangeMonth(modal, new DateOnly(2000, 1, 1));

        // Assert
        Assert.Empty(modal.FindAll("[role='alert']"));
    }

    [Fact]
    public async Task Balance_is_saved_after_correcting_month_following_eligibility_failure()
    {
        // Arrange the modal after an eligibility failure, then correct the month.
        _monthlyBalanceService
            .SetupSequence(service => service.AddMonthlyBalanceAsync(It.IsAny<MonthlyBalanceCreateDTO>()))
            .ReturnsAsync(Result.Failure(MonthlyBalanceErrors.AccountNotEligible))
            .ReturnsAsync(Result.Success());

        var saved = false;
        var visibilityChanges = new List<bool>();
        IRenderedComponent<AddMonthlyBalanceModal> modal = RenderModal(
            () => saved = true,
            visible => visibilityChanges.Add(visible));

        await PressSaveButton(modal);
        await ChangeMonth(modal, new DateOnly(2000, 1, 1));

        // Act
        await PressSaveButton(modal);

        // Assert
        Assert.Multiple(
            () => Assert.True(saved),
            () => Assert.Equal(new[] { false }, visibilityChanges));
    }

    [Fact]
    public async Task Repeat_submission_is_disabled_while_balance_save_is_pending()
    {
        var completion = new TaskCompletionSource<Result>(TaskCreationOptions.RunContinuationsAsynchronously);
        _monthlyBalanceService.Setup(service => service.AddMonthlyBalanceAsync(It.IsAny<MonthlyBalanceCreateDTO>()))
            .Returns(completion.Task);
        IRenderedComponent<AddMonthlyBalanceModal> modal = RenderModal();

        var save = PressSaveButton(modal);

        try
        {
            await modal.WaitForAssertionAsync(() => Assert.True(
                modal.GetByRole("button", "Save Transaction")
                    .HasAttribute("disabled")));
        }
        finally
        {
            completion.SetResult(Result.Success());
            await save;
        }
    }

    private IRenderedComponent<AddMonthlyBalanceModal> RenderModal(
        Action? onSaved = null,
        Action<bool>? onVisibilityChanged = null
    )
    {
        return Render<AddMonthlyBalanceModal>(parameters => parameters.Add(component => component.IsVisible, true)
            .Add(component => component.AccountId, 12)
            .Add(component => component.AccountName, "Savings")
            .Add(component => component.StartingBalance, 100m)
            .Add(
                component => component.IsVisibleChanged,
                onVisibilityChanged
                ?? (_ =>
                {
                }))
            .Add(
                component => component.OnTransactionSaved,
                onSaved
                ?? (() =>
                {
                })));
    }

    private static Task PressSaveButton(IRenderedComponent<AddMonthlyBalanceModal> modal)
    {
        return modal.GetByRole("button", "Save Transaction")
            .ClickAsync(new MouseEventArgs());
    }

    private static Task ChangeMonth(IRenderedComponent<AddMonthlyBalanceModal> modal, DateOnly month)
    {
        return modal.InvokeAsync(async () => await modal.FindComponent<MonthYearPicker>()
            .Instance.ValueChanged.InvokeAsync(month));
    }

    private void SetupMonthlyBalanceServiceError(Error error)
    {
        _monthlyBalanceService.Setup(service => service.AddMonthlyBalanceAsync(It.IsAny<MonthlyBalanceCreateDTO>()))
            .ReturnsAsync(Result.Failure(error));
    }

    private void SetupMonthlyBalanceServiceSuccess()
    {
        _monthlyBalanceService.Setup(service => service.AddMonthlyBalanceAsync(It.IsAny<MonthlyBalanceCreateDTO>()))
            .ReturnsAsync(Result.Success());
    }
}
