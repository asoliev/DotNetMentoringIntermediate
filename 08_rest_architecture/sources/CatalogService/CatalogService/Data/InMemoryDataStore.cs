using CatalogService.Models;
using System.Collections.Concurrent;

namespace CatalogService.Data;

/// <summary>
/// In-memory data store that simulates an external database
/// Loaded once at startup from JSON files
/// </summary>
public class InMemoryDataStore
{
    private readonly ConcurrentDictionary<int, Category> _categories = new();
    private readonly ConcurrentDictionary<int, Item> _items = new();
    private int _nextCategoryId = 1;
    private int _nextItemId = 1;

    public IEnumerable<Category> GetAllCategories()
    {
        return _categories.Values.ToList();
    }

    public Category? GetCategoryById(int id)
    {
        _categories.TryGetValue(id, out var category);
        return category;
    }

    public Category AddCategory(Category category)
    {
        category.Id = Interlocked.Increment(ref _nextCategoryId) - 1;
        _categories[category.Id] = category;
        return category;
    }

    public Category? UpdateCategory(int id, Category category)
    {
        if (!_categories.ContainsKey(id))
            return null;

        category.Id = id;
        _categories[id] = category;
        return category;
    }

    public bool DeleteCategory(int id)
    {
        return _categories.TryRemove(id, out _);
    }

    public IEnumerable<Item> GetAllItems()
    {
        return _items.Values.ToList();
    }

    public Item? GetItemById(int id)
    {
        _items.TryGetValue(id, out var item);
        return item;
    }

    public Item AddItem(Item item)
    {
        item.Id = Interlocked.Increment(ref _nextItemId) - 1;
        _items[item.Id] = item;
        return item;
    }

    public Item? UpdateItem(int id, Item item)
    {
        if (!_items.ContainsKey(id))
            return null;

        item.Id = id;
        _items[id] = item;
        return item;
    }

    public bool DeleteItem(int id)
    {
        return _items.TryRemove(id, out _);
    }

    public int DeleteItemsByCategoryId(int categoryId)
    {
        var itemsToDelete = _items.Values.Where(i => i.CategoryId == categoryId).ToList();
        var count = 0;

        foreach (var item in itemsToDelete)
        {
            if (_items.TryRemove(item.Id, out _))
                count++;
        }

        return count;
    }
}
