namespace NetWorthTracker.Data.Features.Categories;

public class CategoryEntity
{
    public CategoryEntity(
        int id,
        string name,
        string type,
        DateTime? closedDate
    )
    {
        Name = name;
        Id = id;
        Type = type;
        ClosedDate = closedDate;
    }

    public int Id { get; private set; }

    public string Name { get; private set; }

    public string Type { get; private set; }

    public DateTime? ClosedDate { get; private set; }
}
