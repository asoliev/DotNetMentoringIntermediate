using CatalogService.Data;
using CatalogService.Models;

namespace CatalogService.Repositories;

/// <summary>
/// In-memory implementation of Item repository
/// Uses the shared InMemoryDataStore (simulating a database)
/// </summary>
public class InMemoryItemRepository(InMemoryDataStore dataStore) : IItemRepository
{
    public Task<IEnumerable<Item>> GetAllAsync()
    {
        var items = dataStore.GetAllItems();
        return Task.FromResult(items);
    }

    public Task<Item?> GetByIdAsync(int id)
    {
        var item = dataStore.GetItemById(id);
        return Task.FromResult(item);
    }

    public Task<Item> AddAsync(Item item)
    {
        var created = dataStore.AddItem(item);
        return Task.FromResult(created);
    }

    public Task<Item?> UpdateAsync(int id, Item item)
    {
        var updated = dataStore.UpdateItem(id, item);
        return Task.FromResult(updated);
    }

    public Task<bool> DeleteAsync(int id)
    {
        var deleted = dataStore.DeleteItem(id);
        return Task.FromResult(deleted);
    }

    public Task<int> DeleteByCategoryIdAsync(int categoryId)
    {
        var count = dataStore.DeleteItemsByCategoryId(categoryId);
        return Task.FromResult(count);
    }

    public Task<IEnumerable<Item>> GetByCategoryIdAsync(int categoryId)
    {
        var items = dataStore.GetAllItems()
            .Where(i => i.CategoryId == categoryId);
        return Task.FromResult(items);
    }
}
