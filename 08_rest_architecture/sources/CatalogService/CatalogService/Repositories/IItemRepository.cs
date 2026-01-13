using CatalogService.Models;

namespace CatalogService.Repositories;

/// <summary>
/// Repository interface for Item data access
/// </summary>
public interface IItemRepository
{
    Task<IEnumerable<Item>> GetAllAsync();
    Task<Item?> GetByIdAsync(int id);
    Task<Item> AddAsync(Item item);
    Task<Item?> UpdateAsync(int id, Item item);
    Task<bool> DeleteAsync(int id);
    Task<int> DeleteByCategoryIdAsync(int categoryId);
    Task<IEnumerable<Item>> GetByCategoryIdAsync(int categoryId);
}
