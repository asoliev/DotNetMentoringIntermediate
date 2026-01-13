using CatalogService.Models;

namespace CatalogService.Services;

public interface ICatalogService
{
    // Category operations
    Task<IEnumerable<Category>> GetCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(int id);
    Task<Category> CreateCategoryAsync(Category category);
    Task<Category?> UpdateCategoryAsync(int id, Category category);
    Task<bool> DeleteCategoryAsync(int id);
    
    // Item operations
    Task<PagedResult<Item>> GetItemsAsync(int? categoryId = null, int pageNumber = 1, int pageSize = 10);
    Task<Item?> GetItemByIdAsync(int id);
    Task<Item> CreateItemAsync(Item item);
    Task<Item?> UpdateItemAsync(int id, Item item);
    Task<bool> DeleteItemAsync(int id);
}
