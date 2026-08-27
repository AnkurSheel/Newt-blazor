using NetWorthTracker.Core.Features.Categories;

namespace NetWorthTracker.Data.Features.Categories;

public static class CategoryExtensions
{
    public static CategoryDTO ToModel(this CategoryEntity categoryEntity)
        => new(
            categoryEntity.Id,
            categoryEntity.Name,
            Enum.Parse<CategoryType>(categoryEntity.Type),
            categoryEntity.ClosedDate);

    public static CategoryEntity ToEntity(this CategoryDTO categoryDto)
        => new(
            categoryDto.Id,
            categoryDto.Name,
            categoryDto.Type.ToString(),
            categoryDto.ClosedDate);
}
