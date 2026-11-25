using Microsoft.AspNetCore.Components;

using NetWorthTracker.Business.Features.FinancialSummary.Api;

namespace NetWorthTracker.UI.Components;

public partial class FinancialCardSummary : ComponentBase
{
    [Parameter]
    public decimal Amount { get; set; }

    [Parameter]
    public float PercentageChange { get; set; }

    [Parameter]
    public decimal AbsoluteChange { get; set; }

    [Parameter]
    public SummaryType SummaryType { get; set; }

    private string GetChangeCssClass()
    {
        return PercentageChange switch
        {
            0 => "text-foreground-muted bg-background-page border-outline",
            _ => IsPositiveOutcome()
                ? "bg-financial-positive-emphasis/10 text-financial-positive border-financial-positive-emphasis/20"
                : "bg-trend-negative-emphasis/10 text-trend-negative border-trend-negative-emphasis/20"
        };
    }

    private string GetCardCssClass()
    {
        return SummaryType switch
        {
            SummaryType.ASSET => "bg-metric-asset-background/20 text-metric-asset border-metric-asset-outline/20",
            SummaryType.LIABILITY => "bg-metric-liability-background/20 text-metric-liability border-metric-liability-outline/20",
            SummaryType.NETWORTH => "bg-metric-net-worth-background/20 text-metric-net-worth border-metric-net-worth-outline/20",
            _ => "bg-background-page text-foreground-muted border-outline"
        };
    }

    private string GetFormattedAmount(decimal amount)
    {
        return amount.ToString("C0");
    }

    private string GetFormattedPercentageChange()
    {
        var formattedPercentageChange = PercentageChange.ToString("P2");

        return PercentageChange switch
        {
            > 0 => $"↑ {formattedPercentageChange}",
            < 0 => $"↓ {formattedPercentageChange}",
            _ => $"→ {formattedPercentageChange}"
        };
    }

    private bool IsPositiveOutcome()
    {
        return PercentageChange switch
        {
            > 0 => SummaryType == SummaryType.ASSET || SummaryType == SummaryType.NETWORTH,
            < 0 => SummaryType == SummaryType.LIABILITY,
            _ => false
        };
    }

    private string GetTitle()
    {
        return SummaryType switch
        {
            SummaryType.ASSET => "Assets",
            SummaryType.LIABILITY => "Debts",
            SummaryType.NETWORTH => "Networth",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private string GetFormattedAbsoluteChange(decimal absoluteChange)
    {
        var formattedAbsoluteChange = $"+{Math.Abs(absoluteChange):C0}";

        return PercentageChange switch
        {
            0 => formattedAbsoluteChange,
            _ => IsPositiveOutcome() ? formattedAbsoluteChange : $"-{Math.Abs(absoluteChange):C0}"
        };
    }
}
