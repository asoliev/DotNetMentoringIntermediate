using CatalogService.Models;

namespace CatalogService.Services;

public class ItemService(ICategoryService categoryService) : IItemService
{
    private readonly List<Item> _items = [];
    private int _nextItemId = 1;
    private readonly Lock _lock = new();

    public Task<PagedResult<Item>> GetItemsAsync(int? categoryId = null, int pageNumber = 1, int pageSize = 10)
    {
        lock (_lock)
        {
            var query = _items.AsQueryable();

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
    }

    public Task<Item?> GetItemByIdAsync(int id)
    {
        lock (_lock)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                var category = categoryService.GetCategoryByIdAsync(item.CategoryId).Result;
                item.Category = category;
            }
            return Task.FromResult(item);
        }
    }

    public Task<Item> CreateItemAsync(Item item)
    {
        lock (_lock)
        {
            // Validate category exists
            var category = categoryService.GetCategoryByIdAsync(item.CategoryId).Result;
            if (category == null)
                throw new InvalidOperationException($"Category with id {item.CategoryId} does not exist.");

            item.Id = _nextItemId++;
            _items.Add(item);
            item.Category = category;
            return Task.FromResult(item);
        }
    }

    public Task<Item?> UpdateItemAsync(int id, Item item)
    {
        lock (_lock)
        {
            var existingItem = _items.FirstOrDefault(i => i.Id == id);
            if (existingItem == null)
                return Task.FromResult<Item?>(null);

            // Validate category exists
            var category = categoryService.GetCategoryByIdAsync(item.CategoryId).Result;
            if (category == null)
                throw new InvalidOperationException($"Category with id {item.CategoryId} does not exist.");

            existingItem.Name = item.Name;
            existingItem.Description = item.Description;
            existingItem.Price = item.Price;
            existingItem.CategoryId = item.CategoryId;
            existingItem.Category = category;
            
            return Task.FromResult<Item?>(existingItem);
        }
    }

    public Task<bool> DeleteItemAsync(int id)
    {
        lock (_lock)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            if (item == null)
                return Task.FromResult(false);

            _items.Remove(item);
            return Task.FromResult(true);
        }
    }

    public Task<int> DeleteItemsByCategoryIdAsync(int categoryId)
    {
        lock (_lock)
        {
            var itemsToDelete = _items.Where(i => i.CategoryId == categoryId).ToList();
            var count = itemsToDelete.Count;
            
            foreach (var item in itemsToDelete)
                _items.Remove(item);
            
            return Task.FromResult(count);
        }
    }
}