using CsvHelper.Configuration.Attributes;

namespace NetWorthTracker.Seeder;

public class AccountCsvRecord
{
    [Name("#")]
    public int Number { get; set; }

    [Name("Id")]
    public int Id { get; set; }

    [Name("Name")]
    public string Name { get; set; } = string.Empty;

    [Name("Type")]
    public int Type { get; set; }

    [Name("OpenDate")]
    public DateOnly OpenDate { get; set; }

    [Name("ClosedDate")]
    public DateOnly? ClosedDate { get; set; }
}
