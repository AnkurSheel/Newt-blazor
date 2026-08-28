using NetWorthTracker.Core.Exceptions;
using NetWorthTracker.Core.Features.Categories;
using NetWorthTracker.Data.Api.Features.Categories;
using NetWorthTracker.Services.Api.Features.Categories;

namespace NetWorthTracker.Services.Features.Categories;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<CategoryDTO>> GetActiveCategoriesAsync()
    {
        IReadOnlyList<CategoryDTO> categories = await _repository.GetAllAsync();

        return categories.Where(c => c.ClosedDate is null).ToList();
    }

    public async Task AddCategoryAsync(string name, CategoryType type)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("Category name is required.");
        }
        await _repository.AddAsync(new CategoryDTO(0, name.Trim(), type, null));
    }

    public async Task UpdateCategoryAsync(CategoryDTO category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            throw new ValidationException("Category name is required.");
        }
        await _repository.UpdateAsync(category);
    }

    public Task CloseCategoryAsync(CategoryDTO category)
    {
        return _repository.UpdateAsync(category with { ClosedDate = DateTime.UtcNow });
    }
}
