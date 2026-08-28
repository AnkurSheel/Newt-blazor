using NetWorthTracker.Core.Features.Categories;

namespace NetWorthTracker.Services.Api.Features.Categories;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDTO>> GetActiveCategoriesAsync();

    Task AddCategoryAsync(string name, CategoryType type);

    Task UpdateCategoryAsync(CategoryDTO category);

    Task CloseCategoryAsync(CategoryDTO category);
}
