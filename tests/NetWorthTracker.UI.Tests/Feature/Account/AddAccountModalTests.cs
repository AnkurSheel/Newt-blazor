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
    public async Task Date_error_is_shown_by_date_fields_when_closing_month_is_invalid()
    {
        SetupAccountServiceError("Checking", AccountErrors.InvalidDates);
        IRenderedComponent<AddAccountModal> modal = RenderModal();
        await GetAccountNameInput(modal, "Checking");

        await PressSaveButton(modal);

        var dateSection = modal.Find("input[type='checkbox']").Closest("div");
        Assert.Contains(AccountErrors.InvalidDates.Message, dateSection!.TextContent);
    }

    [Fact]
    public async Task Duplicate_name_error_is_shown_and_dialog_stays_open()
    {
        SetupAccountServiceError("Checking", AccountErrors.DuplicateName);

        var saved = false;
        var closed = false;
        IRenderedComponent<AddAccountModal> modal = RenderModal(() => saved = true, visible => closed = !visible);

        await GetAccountNameInput(modal, "Checking");
        await PressSaveButton(modal);

        var error = modal.GetByText(AccountErrors.DuplicateName.Message);
        Assert.Multiple(
            () => Assert.Equal("alert", error.GetAttribute("role")),
            () => Assert.False(saved),
            () => Assert.False(closed));
    }

    [Fact]
    public async Task Blank_name_error_is_shown_and_dialog_stays_open()
    {
        SetupAccountServiceError(string.Empty, AccountErrors.InvalidName);

        var saved = false;
        var closed = false;
        IRenderedComponent<AddAccountModal> modal = RenderModal(() => saved = true, visible => closed = !visible);

        await PressSaveButton(modal);

        var error = modal.GetByText(AccountErrors.InvalidName.Message);
        Assert.Multiple(
            () => Assert.Equal("alert", error.GetAttribute("role")),
            () => Assert.False(saved),
            () => Assert.False(closed));
    }

    [Fact]
    public async Task Account_is_saved_after_duplicate_name_is_corrected()
    {
        // Arrange the dialog after a duplicate-name response, then correct the input.
        SetupAccountServiceError("Checking", AccountErrors.DuplicateName);

        SetupAccountServiceSuccess("Savings");

        var saved = false;
        var closed = false;
        IRenderedComponent<AddAccountModal> modal = RenderModal(() => saved = true, visible => closed = !visible);

        await GetAccountNameInput(modal, "Checking");
        await PressSaveButton(modal);
        await GetAccountNameInput(modal, "Savings");

        // Act
        await PressSaveButton(modal);

        // Assert
        Assert.Multiple(
            () => Assert.True(saved),
            () => Assert.True(closed));
    }

    [Fact]
    public async Task Repeat_submission_is_disabled_while_account_save_is_pending()
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
