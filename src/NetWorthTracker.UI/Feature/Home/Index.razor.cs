namespace NetWorthTracker.UI.Feature.Home;

public partial class Index
{
    protected override void OnInitialized()
    {
        NavigationManager.NavigateTo("/accounts");
    }
}
