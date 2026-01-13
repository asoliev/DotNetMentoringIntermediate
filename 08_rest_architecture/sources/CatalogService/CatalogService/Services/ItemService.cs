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
        var pagedItems = items
            .Skip((pageNumber - 1) * pageSize)
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
        // Validate category exists
        var category = await categoryRepository.GetByIdAsync(item.CategoryId);
        if (category == null)
            throw new InvalidOperationException($"Category with id {item.CategoryId} does not exist.");

        var created = await itemRepository.AddAsync(item);
        created.Category = category;
        return created;
    }

    public async Task<Item?> UpdateItemAsync(int id, Item item)
    {
        // Validate category exists
        var category = await categoryRepository.GetByIdAsync(item.CategoryId);
        if (category == null)
            throw new InvalidOperationException($"Category with id {item.CategoryId} does not exist.");

        var updated = await itemRepository.UpdateAsync(id, item);
        updated?.Category = category;
        
        return updated;
    }

    public Task<bool> DeleteItemAsync(int id) => itemRepository.DeleteAsync(id);
    public Task<int> DeleteItemsByCategoryIdAsync(int categoryId) => itemRepository.DeleteByCategoryIdAsync(categoryId);
}