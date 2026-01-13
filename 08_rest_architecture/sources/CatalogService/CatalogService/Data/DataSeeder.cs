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

    // Extract JSON loading logic
    private static async Task<List<CategoryDto>> LoadCategoryDataAsync()
    {
        var json = await File.ReadAllTextAsync("Data/SeedData/categories.json");
        return JsonSerializer.Deserialize<List<CategoryDto>>(json) ?? [];
    }

    private static async Task<Dictionary<string, List<ItemDto>>> LoadItemDataAsync()
    {
        var json = await File.ReadAllTextAsync("Data/SeedData/items.json");
        return JsonSerializer.Deserialize<Dictionary<string, List<ItemDto>>>(json) ?? [];
    }

    /// <summary>
    /// Initialize category data store from JSON file
    /// </summary>
    /// <returns>Dictionary mapping category names to their entities</returns>
    private static async Task<Dictionary<string, Category>> InitializeCategoriesAsync(
        CategoryDataStore categoryStore, 
        ILogger? logger = null)
    {
        var categories = await LoadCategoryDataAsync();

        if (logger?.IsEnabled(LogLevel.Information) == true)
            logger.LogInformation("Loading {Count} categories", categories.Count);

        var categoryMap = new Dictionary<string, Category>();

        foreach (var categoryDto in categories)
        {
            var category = categoryStore.Add(new Category
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description
            });
            categoryMap[categoryDto.Name] = category;

            if (logger?.IsEnabled(LogLevel.Debug) == true)
                logger.LogDebug("Loaded category: {Name}", category.Name);
        }

        if (logger?.IsEnabled(LogLevel.Information) == true)
            logger.LogInformation("Successfully loaded {Count} categories", categoryMap.Count);

        return categoryMap;
    }

    /// <summary>
    /// Initialize item data store from JSON file
    /// </summary>
    /// <param name="itemStore">The item data store to populate</param>
    /// <param name="categoryMap">Dictionary of category names to entities for FK resolution</param>
    private static async Task InitializeItemsAsync(ItemDataStore itemStore, Dictionary<string, Category> categoryMap)
    {
        var itemsByCategory = await LoadItemDataAsync();

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