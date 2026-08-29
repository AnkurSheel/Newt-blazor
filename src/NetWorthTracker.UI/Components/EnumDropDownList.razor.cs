#region
using Microsoft.AspNetCore.Components;
#endregion

namespace NetWorthTracker.UI.Components;

public partial class EnumDropDownList<TEnum>
{
    private List<EnumDisplayItem> _items = [];

    [Parameter]
    public string Label { get; init; } = string.Empty;

    [Parameter]
    public string Placeholder { get; init; } = "Select an option...";

    [Parameter]
    public TEnum Value { get; init; }

    [Parameter]
    public EventCallback<TEnum> ValueChanged { get; init; }

    [Parameter]
    public TEnum[] Exclude { get; init; } = [];

    [Parameter]
    public bool AutoSelectFirst { get; init; } = true;

    protected override async Task OnInitializedAsync()
    {
        _items = Enum.GetValues<TEnum>()
            .Where(e => !Exclude.Contains(e))
            .Select(e => new EnumDisplayItem(e, FormatEnumText(e.ToString())))
            .ToList();
        if (AutoSelectFirst && _items.Count != 0)
        {
            var defaultSelection = _items.First()
                .Value;
            await ValueChanged.InvokeAsync(defaultSelection);
        }
    }

    private async Task OnValueChanged(TEnum newValue)
    {
        await ValueChanged.InvokeAsync(newValue);
    }

    private string FormatEnumText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        return char.ToUpper(text[0])
               + text.Substring(1)
                   .ToLower();
    }

    public record EnumDisplayItem(TEnum Value, string Text);
}
