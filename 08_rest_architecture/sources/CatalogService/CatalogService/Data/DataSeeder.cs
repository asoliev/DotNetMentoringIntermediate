using CatalogService.Models;
using System.Text.Json;

namespace CatalogService.Data;

public static class DataSeeder
{
	private record CategoryDto(string Name, string Description);
    private record ItemDto(string Name, string Description, decimal Price);

    /// <summary>
    /// Initialize the data store with seed data from JSON files
    /// </summary>
    public static async Task InitializeAsync(InMemoryDataStore dataStore)
    {
        Dictionary<string, Category> categoryMap = await SeedCategories(dataStore);
        await SeedItems(dataStore, categoryMap);
    }
    private static async Task<Dictionary<string, Category>> SeedCategories(InMemoryDataStore dataStore)
    {
        var categoriesJson = await File.ReadAllTextAsync("Data/SeedData/categories.json");
        var categories = JsonSerializer.Deserialize<List<CategoryDto>>(categoriesJson) ?? [];

        // Seed Categories
        var categoryMap = new Dictionary<string, Category>();

        foreach (var categoryDto in categories)
        {
            var category = dataStore.AddCategory(new Category
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description
            });
            categoryMap[categoryDto.Name] = category;
        }

        return categoryMap;
    }
    private static async Task SeedItems(InMemoryDataStore dataStore, Dictionary<string, Category> categoryMap)
    {
        var itemsJson = await File.ReadAllTextAsync("Data/SeedData/items.json");
        var itemsByCategory = JsonSerializer.Deserialize<Dictionary<string, List<ItemDto>>>(itemsJson) ?? [];

        // Seed Items by Category
        foreach (var (categoryName, items) in itemsByCategory)
            if (categoryMap.TryGetValue(categoryName, out var category))
                foreach (var itemDto in items)
                    dataStore.AddItem(new Item
                    {
                        Name = itemDto.Name,
                        Description = itemDto.Description,
                        Price = itemDto.Price,
                        CategoryId = category.Id
                    });
    }
}