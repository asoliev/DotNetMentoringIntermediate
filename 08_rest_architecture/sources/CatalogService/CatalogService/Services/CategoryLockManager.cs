using System.Collections.Concurrent;

namespace CatalogService.Services;

/// <summary>
/// Shared lock manager for synchronizing operations on categories.
/// Prevents race conditions between category deletion and item creation/update.
/// </summary>
internal static class CategoryLockManager
{
    private static readonly ConcurrentDictionary<int, SemaphoreSlim> _categoryLocks = new();

    /// <summary>
    /// Gets or creates a semaphore lock for the specified category ID.
    /// </summary>
    public static SemaphoreSlim GetLock(int categoryId)
    {
        return _categoryLocks.GetOrAdd(categoryId, _ => new SemaphoreSlim(1, 1));
    }

    /// <summary>
    /// Removes and disposes the lock for the specified category ID.
    /// Should be called after a category is successfully deleted.
    /// </summary>
    public static void RemoveLock(int categoryId)
    {
        if (_categoryLocks.TryRemove(categoryId, out var lockToDispose))
        {
            lockToDispose.Dispose();
        }
    }
}
