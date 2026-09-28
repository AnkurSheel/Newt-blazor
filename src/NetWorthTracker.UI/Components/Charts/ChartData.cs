namespace NetWorthTracker.UI.Components.Charts;

public class ChartData
{
    public ChartData(DateOnly period, float value)
    {
        Period = period;
        Value = value;
    }

    public string Color => Value >= 0 ? "#10b981" : "#f43f5e";

    public DateOnly Period { get; }

    public double Value { get; }
}
