namespace NetWorthTracker.Core.Features.FinancialSummary;

public record MonthlyNetworthChangeDTO(
    decimal AbsoluteAssetChange,
    float AssetPercentChange,
    decimal AbsoluteLiabilityChange,
    float LiabilityPercentChange,
    decimal AbsoluteNetWorthChange,
    float NetWorthPercentChange)
{
    public static MonthlyNetworthChangeDTO Default => new(
        0,
        0,
        0,
        0,
        0,
        0);
}
