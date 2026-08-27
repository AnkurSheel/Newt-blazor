using Microsoft.EntityFrameworkCore;

using NetWorthTracker.Core.Features.Categories;
using NetWorthTracker.Data.Api.Features.Categories;

namespace NetWorthTracker.Data.Features.Categories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;

    public CategoryRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CategoryDTO>> GetAllAsync()
    {
        return await _db.Categories.Select(categoryEntity => categoryEntity.ToModel()).ToListAsync();
    }

    public async Task<CategoryDTO?> GetByIdAsync(int id)
    {
        var entity = await _db.Categories.FindAsync(id);

        return entity?.ToModel();
    }

    public async Task AddAsync(CategoryDTO category)
    {
        _db.Categories.Add(category.ToEntity());

        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(CategoryDTO category)
    {
        _db.Categories.Update(category.ToEntity());

        await _db.SaveChangesAsync();
    }
}
