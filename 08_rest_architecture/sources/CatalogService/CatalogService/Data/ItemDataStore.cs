using CatalogService.Models;
using System.Collections.Concurrent;

namespace CatalogService.Data;

/// <summary>
/// In-memory data store for Items (simulates a database table)
/// </summary>
public class ItemDataStore
{
    private readonly ConcurrentDictionary<int, Item> _items = new();
    private int _nextId = 1;

    public IEnumerable<Item> GetAll() => [.. _items.Values];

    public Item? GetById(int id)
    {
        _items.TryGetValue(id, out var item);
        return item;
    }

    public IEnumerable<Item> GetByCategoryId(int categoryId)
    {
        return _items.Values.Where(i => i.CategoryId == categoryId);
    }

    public Item Add(Item item)
    {
        item.Id = Interlocked.Increment(ref _nextId) - 1;
        _items[item.Id] = item;
        return item;
    }

    public Item? Update(int id, Item item)
    {
        // Use TryGetValue to atomically check existence and get the old value
        if (!_items.TryGetValue(id, out var oldItem))
            return null;

        item.Id = id;
        // Use TryUpdate to atomically update only if the key still exists
        // This prevents resurrecting deleted entries
        if (_items.TryUpdate(id, item, oldItem))
            return item;

        // If TryUpdate failed, the entry was deleted between TryGetValue and TryUpdate
        return null;
    }

    public bool Delete(int id) => _items.TryRemove(id, out _);

    public int DeleteByCategoryId(int categoryId)
    {
        var itemsToDelete = _items.Values.Where(i => i.CategoryId == categoryId).ToList();
        var count = 0;

        foreach (var item in itemsToDelete)
            if (_items.TryRemove(item.Id, out _))
                count++;

        return count;
    }
}
