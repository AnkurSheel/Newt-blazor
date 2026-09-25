using CsvHelper.Configuration.Attributes;

namespace NetWorthTracker.Seeder;

public class MonthlyBalancesCsvRecord
{
    [Name("#")]
    public int Number { get; set; }

    [Name("Id")]
    public int Id { get; set; }

    [Name("AccountId")]
    public int AccountId { get; set; }

    [Name("MonthDate")]
    public DateOnly MonthDate { get; set; }

    [Name("Amount")]
    public decimal Amount { get; set; }
}
