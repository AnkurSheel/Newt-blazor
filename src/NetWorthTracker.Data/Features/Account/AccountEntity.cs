using System.ComponentModel.DataAnnotations.Schema;

namespace NetWorthTracker.Data.Features.Account;

public class AccountEntity
{
    public AccountEntity(
        string name,
        string type,
        DateOnly openDate,
        DateOnly? closedDate
    )
    {
        Name = name;
        Type = type;
        ClosedDate = closedDate;
        OpenDate = openDate;
    }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private set; }

    public string Name { get; private set; }

    public string Type { get; private set; }

    public DateOnly OpenDate { get; private set; }

    public DateOnly? ClosedDate { get; private set; }
}
