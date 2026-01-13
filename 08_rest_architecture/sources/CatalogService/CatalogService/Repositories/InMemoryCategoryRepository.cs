using CatalogService.Data;
using CatalogService.Models;

namespace CatalogService.Repositories;

/// <summary>
/// In-memory implementation of Category repository
/// Uses CategoryDataStore (simulating a database table)
/// </summary>
public class InMemoryCategoryRepository(CategoryDataStore dataStore) : ICategoryRepository
{
    public Task<IEnumerable<Category>> GetAllAsync()
    {
        var categories = dataStore.GetAll();
        return Task.FromResult(categories);
    }

    public Task<Category?> GetByIdAsync(int id)
    {
        var category = dataStore.GetById(id);
        return Task.FromResult(category);
    }

    public Task<Category> AddAsync(Category category)
    {
        var created = dataStore.Add(category);
        return Task.FromResult(created);
    }

    public Task<Category?> UpdateAsync(int id, Category category)
    {
        var updated = dataStore.Update(id, category);
        return Task.FromResult(updated);
    }

    public Task<bool> DeleteAsync(int id)
    {
        var deleted = dataStore.Delete(id);
        return Task.FromResult(deleted);
    }
}