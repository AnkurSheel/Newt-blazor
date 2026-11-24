using AngleSharp.Html.Dom;

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
    public async Task Save_WhenAccountNameAlreadyExists_ShowsErrorAndKeepsDialogOpen()
    {
        // Arrange
        _accountServiceMock.Setup(s => s.AddAccountAsync(It.Is<AccountCreateDTO>(dto => dto.Name == "Checking")))
            .ReturnsAsync(Result.Failure(AccountErrors.DuplicateName));

        var saved = false;
        var closed = false;
        IRenderedComponent<AddAccountModal> modal = RenderModal(() => saved = true, visible => closed = !visible);
        await GetAccountNameInput(modal)
            .ChangeAsync(new ChangeEventArgs { Value = "Checking" });

        // Act
        await modal.GetByRole("button", "Save Account")
            .ClickAsync(new MouseEventArgs());

        // Assert
        var error = modal.GetByText("This account has already been added.");
        Assert.Equal("alert", error.GetAttribute("role"));
        Assert.Equal("This account has already been added.", error.TextContent.Trim());
        Assert.Equal(
            "Checking",
            GetAccountNameInput(modal)
                .Value);
        Assert.False(saved);
        Assert.False(closed);
    }

    [Fact]
    public async Task Save_WhenAccountNameIsBlank_ShowsValidationErrorAndKeepsDialogOpen()
    {
        // Arrange
        _accountServiceMock.Setup(s
                => s.AddAccountAsync(It.Is<AccountCreateDTO>(dto => string.IsNullOrWhiteSpace(dto.Name))))
            .ReturnsAsync(Result.Failure(AccountErrors.InvalidName));

        var saved = false;
        var closed = false;
        IRenderedComponent<AddAccountModal> modal = RenderModal(() => saved = true, visible => closed = !visible);

        // Act
        await modal.GetByRole("button", "Save Account")
            .ClickAsync(new MouseEventArgs());

        // Assert
        var error = modal.GetByText(AccountErrors.InvalidName.Message);
        Assert.Equal("alert", error.GetAttribute("role"));
        Assert.Equal(AccountErrors.InvalidName.Message, error.TextContent.Trim());
        Assert.False(saved);
        Assert.False(closed);
    }

    [Fact]
    public async Task Save_WhenDuplicateNameIsCorrected_SavesAccountAndClosesDialog()
    {
        // Arrange
        _accountServiceMock.Setup(s => s.AddAccountAsync(It.Is<AccountCreateDTO>(dto => dto.Name == "Checking")))
            .ReturnsAsync(Result.Failure(AccountErrors.DuplicateName));

        _accountServiceMock.Setup(s => s.AddAccountAsync(It.Is<AccountCreateDTO>(dto => dto.Name == "Savings")))
            .ReturnsAsync(Result.Success());

        var saved = false;
        var closed = false;
        IRenderedComponent<AddAccountModal> modal = RenderModal(() => saved = true, visible => closed = !visible);

        await GetAccountNameInput(modal)
            .ChangeAsync(new ChangeEventArgs { Value = "Checking" });
        await modal.GetByRole("button", "Save Account")
            .ClickAsync(new MouseEventArgs());
        await GetAccountNameInput(modal)
            .ChangeAsync(new ChangeEventArgs { Value = "Savings" });

        // Act
        await modal.GetByRole("button", "Save Account")
            .ClickAsync(new MouseEventArgs());

        // Assert
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

        IRenderedComponent<AddAccountModal> modal = RenderModal(
            () =>
            {
            },
            _ =>
            {
            });
        await GetAccountNameInput(modal)
            .ChangeAsync(new ChangeEventArgs { Value = "Savings" });

        // Act
        var save = modal.GetByRole("button", "Save Account")
            .ClickAsync(new MouseEventArgs());

        // Assert
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

    private static IHtmlInputElement GetAccountNameInput(IRenderedComponent<AddAccountModal> modal)
    {
        return (IHtmlInputElement)modal.GetByLabelText("Account Name");
    }

    private IRenderedComponent<AddAccountModal> RenderModal(Action onSaved, Action<bool> onVisibilityChanged)
    {
        return Render<AddAccountModal>(parameters => parameters.Add(component => component.IsVisible, true)
            .Add(component => component.OnAccountSaved, onSaved)
            .Add(component => component.IsVisibleChanged, onVisibilityChanged));
    }
}
