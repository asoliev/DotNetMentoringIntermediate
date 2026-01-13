using CatalogService.Models;
using System.Text.Json;

namespace CatalogService.Data;

public static class DataSeeder
{
    private record CategoryDto(string Name, string Description);
    private record ItemDto(string Name, string Description, decimal Price);

    /// <summary>
    /// Initialize both data stores with seed data from JSON files
    /// </summary>
    public static async Task InitializeAsync(CategoryDataStore categoryStore, ItemDataStore itemStore)
    {
        var categoryMap = await InitializeCategoriesAsync(categoryStore);
        await InitializeItemsAsync(itemStore, categoryMap);
    }

    /// <summary>
    /// Initialize category data store from JSON file
    /// </summary>
    /// <returns>Dictionary mapping category names to their entities</returns>
    private static async Task<Dictionary<string, Category>> InitializeCategoriesAsync(CategoryDataStore categoryStore)
    {
        var categoriesJson = await File.ReadAllTextAsync("Data/SeedData/categories.json");
        var categories = JsonSerializer.Deserialize<List<CategoryDto>>(categoriesJson) ?? [];

        var categoryMap = new Dictionary<string, Category>();

        foreach (var categoryDto in categories)
        {
            var category = categoryStore.Add(new Category
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description
            });
            categoryMap[categoryDto.Name] = category;
        }

        return categoryMap;
    }

    /// <summary>
    /// Initialize item data store from JSON file
    /// </summary>
    /// <param name="itemStore">The item data store to populate</param>
    /// <param name="categoryMap">Dictionary of category names to entities for FK resolution</param>
    private static async Task InitializeItemsAsync(ItemDataStore itemStore, Dictionary<string, Category> categoryMap)
    {
        var itemsJson = await File.ReadAllTextAsync("Data/SeedData/items.json");
        var itemsByCategory = JsonSerializer.Deserialize<Dictionary<string, List<ItemDto>>>(itemsJson) ?? [];

        foreach (var (categoryName, items) in itemsByCategory)
        {
            if (categoryMap.TryGetValue(categoryName, out var category))
            {
                foreach (var itemDto in items)
                {
                    itemStore.Add(new Item
                    {
                        Name = itemDto.Name,
                        Description = itemDto.Description,
                        Price = itemDto.Price,
                        CategoryId = category.Id
                    });
                }
            }
        }
    }
}