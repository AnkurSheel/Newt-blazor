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
    public async Task Save_WhenAccountIsNotEligible_ShowsErrorAndKeepsModalOpen()
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
    public async Task Save_WhenSuccessful_InvokesSavedCallbackAndClosesModal()
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
    public async Task ChangeMonth_AfterEligibilityFailure_ClearsError()
    {
        SetupMonthlyBalanceServiceError(MonthlyBalanceErrors.AccountNotEligible);
        IRenderedComponent<AddMonthlyBalanceModal> modal = RenderModal();
        await PressSaveButton(modal);

        await ChangeMonth(modal, new DateOnly(2000, 1, 1));

        Assert.Empty(modal.FindAll("[role='alert']"));
    }

    [Fact]
    public async Task Save_WhenMonthIsCorrectedAfterEligibilityFailure_SavesAndClosesModal()
    {
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

        await PressSaveButton(modal);

        Assert.Multiple(
            () => Assert.True(saved),
            () => Assert.Equal(new[] { false }, visibilityChanges));
    }

    [Fact]
    public async Task Save_WhileRequestIsPending_DisablesRepeatSubmission()
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
