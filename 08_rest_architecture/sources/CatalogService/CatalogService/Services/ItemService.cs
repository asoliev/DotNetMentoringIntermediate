using CatalogService.Models;
using CatalogService.Repositories;

namespace CatalogService.Services;

public class ItemService(IItemRepository itemRepository, ICategoryRepository categoryRepository) : IItemService
{
    public async Task<PagedResult<Item>> GetItemsAsync(int? categoryId = null, int pageNumber = 1, int pageSize = 10)
    {
        IEnumerable<Item> items;
        
        if (categoryId.HasValue)
            items = await itemRepository.GetByCategoryIdAsync(categoryId.Value);
        else
            items = await itemRepository.GetAllAsync();

        var totalCount = items.Count();
        
        // Calculate skip value using long to prevent integer overflow
        // If overflow occurs, skip will be negative or exceed int.MaxValue
        long skip = (long)(pageNumber - 1) * pageSize;
        
        // If skip is negative (overflow) or exceeds int.MaxValue, return empty page
        // Also optimize: if skip >= totalCount, no items to return
        if (skip < 0 || skip > int.MaxValue || skip >= totalCount)
        {
            return new PagedResult<Item>
            {
                Items = [],
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        
        var pagedItems = items
            .Skip((int)skip)
            .Take(pageSize)
            .ToList();

        // Load category for each item
        foreach (var item in pagedItems)
        {
            var category = await categoryRepository.GetByIdAsync(item.CategoryId);
            item.Category = category;
        }

        var result = new PagedResult<Item>
        {
            Items = pagedItems,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return result;
    }

    public async Task<Item?> GetItemByIdAsync(int id)
    {
        var item = await itemRepository.GetByIdAsync(id);
        if (item != null)
        {
            var category = await categoryRepository.GetByIdAsync(item.CategoryId);
            item.Category = category;
        }
        return item;
    }

    public async Task<Item> CreateItemAsync(Item item)
    {
        // Get or create a lock for this specific category to prevent race conditions
        // This ensures that if a category is being deleted, we wait for that operation
        // to complete before creating an item, preventing orphaned items
        var categoryLock = CategoryLockManager.GetLock(item.CategoryId);
        
        await categoryLock.WaitAsync();
        try
        {
            // Validate category exists (double-check after acquiring lock)
            // This prevents creating items for categories that are being deleted
            var category = await categoryRepository.GetByIdAsync(item.CategoryId);
            if (category == null)
                throw new InvalidOperationException($"Category with id {item.CategoryId} does not exist.");

            var created = await itemRepository.AddAsync(item);
            created.Category = category;
            return created;
        }
        finally
        {
            categoryLock.Release();
        }
    }

    public async Task<Item?> UpdateItemAsync(int id, Item item)
    {
        // Get or create a lock for this specific category to prevent race conditions
        // This ensures that if a category is being deleted, we wait for that operation
        // to complete before updating an item, preventing orphaned items
        var categoryLock = CategoryLockManager.GetLock(item.CategoryId);
        
        await categoryLock.WaitAsync();
        try
        {
            // Validate category exists (double-check after acquiring lock)
            // This prevents updating items to reference categories that are being deleted
            var category = await categoryRepository.GetByIdAsync(item.CategoryId);
            if (category == null)
                throw new InvalidOperationException($"Category with id {item.CategoryId} does not exist.");

            var updated = await itemRepository.UpdateAsync(id, item);
            updated?.Category = category;
            
            return updated;
        }
        finally
        {
            categoryLock.Release();
        }
    }

    public Task<bool> DeleteItemAsync(int id) => itemRepository.DeleteAsync(id);
    public Task<int> DeleteItemsByCategoryIdAsync(int categoryId) => itemRepository.DeleteByCategoryIdAsync(categoryId);
}