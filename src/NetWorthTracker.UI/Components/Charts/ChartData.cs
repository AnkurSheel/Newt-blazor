namespace NetWorthTracker.UI.Components.Charts;

public class ChartData
{
    public ChartData(DateOnly period, float value)
    {
        Period = period;
        Value = value;
    }

    public string Color => Value >= 0 ? "var(--color-chart-positive)" : "var(--color-chart-negative)";

    public DateOnly Period { get; }

    public double Value { get; }
}
