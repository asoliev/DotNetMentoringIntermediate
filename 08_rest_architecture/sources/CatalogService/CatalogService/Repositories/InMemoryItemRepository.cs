using CatalogService.Data;
using CatalogService.Models;

namespace CatalogService.Repositories;

/// <summary>
/// In-memory implementation of Item repository
/// Uses ItemDataStore (simulating a database table)
/// </summary>
public class InMemoryItemRepository(ItemDataStore dataStore) : IItemRepository
{
    public Task<IEnumerable<Item>> GetAllAsync()
    {
        var items = dataStore.GetAll();
        return Task.FromResult(items);
    }

    public Task<Item?> GetByIdAsync(int id)
    {
        var item = dataStore.GetById(id);
        return Task.FromResult(item);
    }

    public Task<Item> AddAsync(Item item)
    {
        var created = dataStore.Add(item);
        return Task.FromResult(created);
    }

    public Task<Item?> UpdateAsync(int id, Item item)
    {
        var updated = dataStore.Update(id, item);
        return Task.FromResult(updated);
    }

    public Task<bool> DeleteAsync(int id)
    {
        var deleted = dataStore.Delete(id);
        return Task.FromResult(deleted);
    }

    public Task<int> DeleteByCategoryIdAsync(int categoryId)
    {
        var count = dataStore.DeleteByCategoryId(categoryId);
        return Task.FromResult(count);
    }

    public Task<IEnumerable<Item>> GetByCategoryIdAsync(int categoryId)
    {
        var items = dataStore.GetByCategoryId(categoryId);
        return Task.FromResult(items);
    }
}
