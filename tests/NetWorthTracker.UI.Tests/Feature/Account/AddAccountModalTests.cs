using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using NetWorthTracker.Business.Features.Accounts.Api;
using NetWorthTracker.Business.Results;
using NetWorthTracker.UI.Feature.Account;
using NetWorthTracker.UI.Tests.TestHelpers;

namespace NetWorthTracker.UI.Tests.Feature.Account;

public sealed class AddAccountModalTests : BunitContext
{
    private readonly Mock<IAccountService> _accountServiceMock = new();

    public AddAccountModalTests()
    {
        ServiceRegistry.ConfigureServices(Services);
        Services.AddSingleton(_accountServiceMock.Object);
        SetRendererInfo(new RendererInfo("WebView", true));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task Save_WhenAccountNameAlreadyExists_ShowsError()
    {
        SetupAccountServiceError("Checking", AccountErrors.DuplicateName);

        var saved = false;
        var closed = false;
        IRenderedComponent<AddAccountModal> modal = RenderModal(() => saved = true, visible => closed = !visible);

        await GetAccountNameInput(modal, "Checking");
        await PressSaveButton(modal);

        var error = modal.GetByText(AccountErrors.DuplicateName.Message);
        Assert.Equal("alert", error.GetAttribute("role"));
        Assert.False(saved);
        Assert.False(closed);
    }

    [Fact]
    public async Task Save_WhenAccountNameIsBlank_ShowsValidationErrorAndKeepsDialogOpen()
    {
        SetupAccountServiceError(string.Empty, AccountErrors.InvalidName);

        var saved = false;
        var closed = false;
        IRenderedComponent<AddAccountModal> modal = RenderModal(() => saved = true, visible => closed = !visible);

        await PressSaveButton(modal);

        var error = modal.GetByText(AccountErrors.InvalidName.Message);
        Assert.Equal("alert", error.GetAttribute("role"));
        Assert.False(saved);
        Assert.False(closed);
    }

    [Fact]
    public async Task Save_WhenDuplicateNameIsCorrected_SavesAccountAndClosesDialog()
    {
        SetupAccountServiceError("Checking", AccountErrors.DuplicateName);

        SetupAccountServiceSuccess("Savings");

        var saved = false;
        var closed = false;
        IRenderedComponent<AddAccountModal> modal = RenderModal(() => saved = true, visible => closed = !visible);

        await GetAccountNameInput(modal, "Checking");
        await PressSaveButton(modal);
        await GetAccountNameInput(modal, "Savings");

        await PressSaveButton(modal);

        Assert.True(saved);
        Assert.True(closed);
    }

    [Fact]
    public async Task Save_WhileRequestIsPending_DisablesRepeatSubmission()
    {
        // Arrange
        var completion = new TaskCompletionSource<Result>(TaskCreationOptions.RunContinuationsAsynchronously);
        _accountServiceMock.Setup(s => s.AddAccountAsync(It.IsAny<AccountCreateDTO>()))
            .Returns(completion.Task);

        IRenderedComponent<AddAccountModal> modal = RenderModal();
        await GetAccountNameInput(modal, "Savings");

        var save = PressSaveButton(modal);

        try
        {
            await modal.WaitForAssertionAsync(() => Assert.True(
                modal.GetByRole("button", "Save Account")
                    .HasAttribute("disabled")));
        }
        finally
        {
            completion.SetResult(Result.Success());
            await save;
        }
    }

    private static Task GetAccountNameInput(IRenderedComponent<AddAccountModal> modal, object? accountName)
    {
        return modal.GetByLabelText("Account Name")
            .ChangeAsync(new ChangeEventArgs { Value = accountName });
    }

    private IRenderedComponent<AddAccountModal> RenderModal(
        Action? onSaved = null,
        Action<bool>? onVisibilityChanged = null
    )
    {
        return Render<AddAccountModal>(parameters => parameters.Add(component => component.IsVisible, true)
            .Add(
                component => component.OnAccountSaved,
                onSaved
                ?? (() =>
                {
                }))
            .Add(
                component => component.IsVisibleChanged,
                onVisibilityChanged
                ?? (_ =>
                {
                })));
    }

    private void SetupAccountServiceError(string accountName, Error error)
    {
        _accountServiceMock.Setup(s => s.AddAccountAsync(It.Is<AccountCreateDTO>(dto => dto.Name == accountName)))
            .ReturnsAsync(Result.Failure(error));
    }

    private void SetupAccountServiceSuccess(string accountName)
    {
        _accountServiceMock.Setup(s => s.AddAccountAsync(It.Is<AccountCreateDTO>(dto => dto.Name == accountName)))
            .ReturnsAsync(Result.Success);
    }

    private static Task PressSaveButton(IRenderedComponent<AddAccountModal> modal)
    {
        var save = modal.GetByRole("button", "Save Account")
            .ClickAsync(new MouseEventArgs());

        return save;
    }
}
