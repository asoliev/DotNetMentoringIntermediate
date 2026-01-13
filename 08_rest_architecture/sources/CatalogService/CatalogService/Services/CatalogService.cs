using CatalogService.Models;

namespace CatalogService.Services;

public class CatalogService : ICatalogService
{
    private readonly List<Category> _categories = new();
    private readonly List<Item> _items = new();
    private int _nextCategoryId = 1;
    private int _nextItemId = 1;
    private readonly object _lock = new();

    public Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        lock (_lock)
        {
            return Task.FromResult<IEnumerable<Category>>(_categories.ToList());
        }
    }

    public Task<Category?> GetCategoryByIdAsync(int id)
    {
        lock (_lock)
        {
            return Task.FromResult(_categories.FirstOrDefault(c => c.Id == id));
        }
    }

    public Task<Category> CreateCategoryAsync(Category category)
    {
        lock (_lock)
        {
            category.Id = _nextCategoryId++;
            _categories.Add(category);
            return Task.FromResult(category);
        }
    }

    public Task<Category?> UpdateCategoryAsync(int id, Category category)
    {
        lock (_lock)
        {
            var existingCategory = _categories.FirstOrDefault(c => c.Id == id);
            if (existingCategory == null)
            {
                return Task.FromResult<Category?>(null);
            }

            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;
            return Task.FromResult<Category?>(existingCategory);
        }
    }

    public Task<bool> DeleteCategoryAsync(int id)
    {
        lock (_lock)
        {
            var category = _categories.FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return Task.FromResult(false);
            }

            // Delete all related items
            _items.RemoveAll(i => i.CategoryId == id);
            
            // Delete the category
            _categories.Remove(category);
            return Task.FromResult(true);
        }
    }

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
                item.Category = _categories.FirstOrDefault(c => c.Id == item.CategoryId);
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
                item.Category = _categories.FirstOrDefault(c => c.Id == item.CategoryId);
            }
            return Task.FromResult(item);
        }
    }

    public Task<Item> CreateItemAsync(Item item)
    {
        lock (_lock)
        {
            // Validate category exists
            if (!_categories.Any(c => c.Id == item.CategoryId))
            {
                throw new InvalidOperationException($"Category with id {item.CategoryId} does not exist.");
            }

            item.Id = _nextItemId++;
            _items.Add(item);
            item.Category = _categories.FirstOrDefault(c => c.Id == item.CategoryId);
            return Task.FromResult(item);
        }
    }

    public Task<Item?> UpdateItemAsync(int id, Item item)
    {
        lock (_lock)
        {
            var existingItem = _items.FirstOrDefault(i => i.Id == id);
            if (existingItem == null)
            {
                return Task.FromResult<Item?>(null);
            }

            // Validate category exists
            if (!_categories.Any(c => c.Id == item.CategoryId))
            {
                throw new InvalidOperationException($"Category with id {item.CategoryId} does not exist.");
            }

            existingItem.Name = item.Name;
            existingItem.Description = item.Description;
            existingItem.Price = item.Price;
            existingItem.CategoryId = item.CategoryId;
            existingItem.Category = _categories.FirstOrDefault(c => c.Id == item.CategoryId);
            
            return Task.FromResult<Item?>(existingItem);
        }
    }

    public Task<bool> DeleteItemAsync(int id)
    {
        lock (_lock)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            if (item == null)
            {
                return Task.FromResult(false);
            }

            _items.Remove(item);
            return Task.FromResult(true);
        }
    }
}
