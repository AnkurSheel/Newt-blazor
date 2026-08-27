using NetWorthTracker.Core.Features.Categories;

namespace NetWorthTracker.Data.Api.Features.Categories;

public interface ICategoryRepository
{
    Task<IReadOnlyList<CategoryDTO>> GetAllAsync();

    Task<CategoryDTO?> GetByIdAsync(int id);

    Task AddAsync(CategoryDTO category);

    Task UpdateAsync(CategoryDTO category);
}
