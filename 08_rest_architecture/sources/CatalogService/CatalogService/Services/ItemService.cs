using CatalogService.Data;
using CatalogService.Models;

namespace CatalogService.Services;

public class ItemService(InMemoryDataStore dataStore, ICategoryService categoryService) : IItemService
{
    public Task<PagedResult<Item>> GetItemsAsync(int? categoryId = null, int pageNumber = 1, int pageSize = 10)
    {
        var query = dataStore.GetAllItems().AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(i => i.CategoryId == categoryId.Value);
        }

        var totalCount = query.Count();
        var items = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Load category for each item
        foreach (var item in items)
        {
            var category = categoryService.GetCategoryByIdAsync(item.CategoryId).Result;
            item.Category = category;
        }

        var result = new PagedResult<Item>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Task.FromResult(result);
    }

    public Task<Item?> GetItemByIdAsync(int id)
    {
        var item = dataStore.GetItemById(id);
        if (item != null)
        {
            var category = categoryService.GetCategoryByIdAsync(item.CategoryId).Result;
            item.Category = category;
        }
        return Task.FromResult(item);
    }

    public Task<Item> CreateItemAsync(Item item)
    {
        // Validate category exists
        var category = categoryService.GetCategoryByIdAsync(item.CategoryId).Result;
        if (category == null)
            throw new InvalidOperationException($"Category with id {item.CategoryId} does not exist.");

        var created = dataStore.AddItem(item);
        created.Category = category;
        return Task.FromResult(created);
    }

    public Task<Item?> UpdateItemAsync(int id, Item item)
    {
        // Validate category exists
        var category = categoryService.GetCategoryByIdAsync(item.CategoryId).Result;
        if (category == null)
            throw new InvalidOperationException($"Category with id {item.CategoryId} does not exist.");

        var updated = dataStore.UpdateItem(id, item);
        if (updated != null)
        {
            updated.Category = category;
        }
        
        return Task.FromResult(updated);
    }

    public Task<bool> DeleteItemAsync(int id)
    {
        var deleted = dataStore.DeleteItem(id);
        return Task.FromResult(deleted);
    }

    public Task<int> DeleteItemsByCategoryIdAsync(int categoryId)
    {
        var count = dataStore.DeleteItemsByCategoryId(categoryId);
        return Task.FromResult(count);
    }
}