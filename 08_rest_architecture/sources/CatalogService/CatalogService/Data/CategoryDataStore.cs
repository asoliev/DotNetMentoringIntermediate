using CatalogService.Models;
using System.Collections.Concurrent;

namespace CatalogService.Data;

/// <summary>
/// In-memory data store for Categories (simulates a database table)
/// </summary>
public class CategoryDataStore
{
    private readonly ConcurrentDictionary<int, Category> _categories = new();
    private int _nextId = 1;

    public IEnumerable<Category> GetAll() => [.. _categories.Values];

    public Category? GetById(int id)
    {
        _categories.TryGetValue(id, out var category);
        return category;
    }

    public Category Add(Category category)
    {
        category.Id = Interlocked.Increment(ref _nextId) - 1;
        _categories[category.Id] = category;
        return category;
    }

    public Category? Update(int id, Category category)
    {
        // Use TryGetValue to atomically check existence and get the old value
        if (!_categories.TryGetValue(id, out var oldCategory))
            return null;

        category.Id = id;
        // Use TryUpdate to atomically update only if the key still exists
        // This prevents resurrecting deleted entries
        if (_categories.TryUpdate(id, category, oldCategory))
            return category;

        // If TryUpdate failed, the entry was deleted between TryGetValue and TryUpdate
        return null;
    }

    public bool Delete(int id) => _categories.TryRemove(id, out _);

    public bool Exists(int id) => _categories.ContainsKey(id);
}
