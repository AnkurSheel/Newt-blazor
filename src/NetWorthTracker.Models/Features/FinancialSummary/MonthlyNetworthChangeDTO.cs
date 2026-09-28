namespace NetWorthTracker.Core.Features.FinancialSummary;

public record MonthlyNetworthChangeDTO(
    DateOnly SelectedDate,
    decimal AbsoluteAssetChange,
    float AssetPercentChange,
    decimal AbsoluteLiabilityChange,
    float LiabilityPercentChange,
    decimal AbsoluteNetWorthChange,
    float NetWorthPercentChange)
{
    public static MonthlyNetworthChangeDTO Default => new(
        DateOnly.MinValue,
        0,
        0,
        0,
        0,
        0,
        0);
}
