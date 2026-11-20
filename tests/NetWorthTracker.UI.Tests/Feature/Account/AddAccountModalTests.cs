using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

using NetWorthTracker.Business.Features.Accounts.Api;
using NetWorthTracker.UI.Feature.Account;

using Syncfusion.Blazor.Inputs;

namespace NetWorthTracker.UI.Tests.Feature.Account;

public sealed class AddAccountModalTests : BunitContext
{
    [Fact]
    public async Task Save_WhenAccountNameAlreadyExists_ShowsErrorAndKeepsDialogOpen()
    {
        // Arrange
        var saved = false;
        var closed = false;
        var modal = RenderModal(() => saved = true, visible => closed = !visible);
        await SetAccountName(modal, "Checking");

        // Act
        await ClickSave(modal);

        // Assert
        Assert.Equal("This account has already been added.", modal.Find("p[role='alert']").TextContent);
        Assert.Equal("Checking", modal.FindComponent<SfTextBox>().Instance.Value);
        Assert.False(saved);
        Assert.False(closed);
    }

    [Fact]
    public async Task Save_WhenDuplicateNameIsCorrected_SavesAccountAndClosesDialog()
    {
        // Arrange
        var saved = false;
        var closed = false;
        var modal = RenderModal(() => saved = true, visible => closed = !visible);
        await SetAccountName(modal, "Checking");
        await ClickSave(modal);
        await SetAccountName(modal, "Savings");

        // Act
        await ClickSave(modal);

        // Assert
        Assert.True(saved);
        Assert.True(closed);
    }

    [Fact]
    public async Task Save_WhileRequestIsPending_DisablesRepeatSubmission()
    {
        // Arrange
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var modal = RenderModal(() => { }, _ => { }, new TestAccountService(_ => completion.Task));
        await SetAccountName(modal, "Savings");

        // Act
        var save = ClickSave(modal);

        // Assert
        try
        {
            modal.WaitForAssertion(() => Assert.True(FindSaveButton(modal).HasAttribute("disabled")));
        }
        finally
        {
            completion.SetResult();
            await save;
        }
    }

    private static Task SetAccountName(IRenderedComponent<AddAccountModal> modal, string name)
    {
        return modal.InvokeAsync(() => modal.FindComponent<SfTextBox>().Instance.ValueChanged.InvokeAsync(name));
    }

    private static Task ClickSave(IRenderedComponent<AddAccountModal> modal)
    {
        return FindSaveButton(modal).ClickAsync(new MouseEventArgs());
    }

    private static AngleSharp.Dom.IElement FindSaveButton(IRenderedComponent<AddAccountModal> modal)
    {
        return modal.FindAll("button").Single(button => button.TextContent.Trim() == "Save Account");
    }

    private IRenderedComponent<AddAccountModal> RenderModal(
        Action onSaved, Action<bool> onVisibilityChanged, IAccountService? accountService = null)
    {
        ServiceRegistry.ConfigureServices(Services);
        Services.AddSingleton(accountService ?? new TestAccountService());
        SetRendererInfo(new RendererInfo("WebView", true));
        JSInterop.Mode = JSRuntimeMode.Loose;
        return Render<AddAccountModal>(parameters => parameters
            .Add(component => component.IsVisible, true)
            .Add(component => component.OnAccountSaved, onSaved)
            .Add(component => component.IsVisibleChanged, onVisibilityChanged));
    }

    private sealed class TestAccountService(Func<AccountCreateDTO, Task>? addAccount = null) : IAccountService
    {
        public Task<IReadOnlyList<AccountResponseDTO>> GetAccountsAsync(DateOnly selectedDate)
        {
            throw new NotSupportedException();
        }

        public Task AddAccountAsync(AccountCreateDTO account)
        {
            return addAccount?.Invoke(account) ?? (account.Name == "Checking"
                ? Task.FromException(new DuplicateAccountException(new InvalidOperationException()))
                : Task.CompletedTask);
        }
    }
}
