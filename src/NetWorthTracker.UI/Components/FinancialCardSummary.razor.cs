using Microsoft.AspNetCore.Components;

using NetWorthTracker.Core.Features.FinancialSummary;

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
            0 => "text-slate-400 bg-slate-900 border-slate-800",
            _ => IsPositiveOutcome()
                ? "bg-emerald-500/10 text-emerald-400 border-emerald-500/20"
                : "bg-red-500/10 text-red-400 border-red-500/20"
        };
    }

    private string GetCardCssClass()
    {
        return SummaryType switch
        {
            SummaryType.ASSET => "bg-teal-800/20 text-teal-400 border-teal-500/20",
            SummaryType.LIABILITY => "bg-rose-800/20 text-rose-400 border-rose-500/20",
            SummaryType.NETWORTH => "bg-violet-800/20 text-violet-400 border-violet-500/20",
            _ => "bg-slate-900 text-slate-400 border-slate-800"
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
