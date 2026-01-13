using CatalogService.Models;

namespace CatalogService.Services;

public interface IItemService
{
    Task<PagedResult<Item>> GetItemsAsync(int? categoryId = null, int pageNumber = 1, int pageSize = 10);
    Task<Item?> GetItemByIdAsync(int id);
    Task<Item> CreateItemAsync(Item item);
    Task<Item?> UpdateItemAsync(int id, Item item);
    Task<bool> DeleteItemAsync(int id);
    Task<int> DeleteItemsByCategoryIdAsync(int categoryId);
}
