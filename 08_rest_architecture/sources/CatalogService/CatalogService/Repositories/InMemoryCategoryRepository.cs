using CatalogService.Data;
using CatalogService.Models;

namespace CatalogService.Repositories;

/// <summary>
/// In-memory implementation of Category repository
/// Uses the shared InMemoryDataStore (simulating a database)
/// </summary>
public class InMemoryCategoryRepository(InMemoryDataStore dataStore) : ICategoryRepository
{
    public Task<IEnumerable<Category>> GetAllAsync()
    {
        var categories = dataStore.GetAllCategories();
        return Task.FromResult(categories);
    }

    public Task<Category?> GetByIdAsync(int id)
    {
        var category = dataStore.GetCategoryById(id);
        return Task.FromResult(category);
    }

    public Task<Category> AddAsync(Category category)
    {
        var created = dataStore.AddCategory(category);
        return Task.FromResult(created);
    }

    public Task<Category?> UpdateAsync(int id, Category category)
    {
        var updated = dataStore.UpdateCategory(id, category);
        return Task.FromResult(updated);
    }

    public Task<bool> DeleteAsync(int id)
    {
        var deleted = dataStore.DeleteCategory(id);
        return Task.FromResult(deleted);
    }
}
