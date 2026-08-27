namespace NetWorthTracker.Core.Features.Categories;

public record CategoryDTO(
    int Id,
    string Name,
    CategoryType Type,
    DateTime? ClosedDate);
