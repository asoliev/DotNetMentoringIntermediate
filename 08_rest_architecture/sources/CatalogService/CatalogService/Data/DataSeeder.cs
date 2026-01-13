using CatalogService.Models;
using CatalogService.Services;
using System.Text.Json;

namespace CatalogService.Data;

public static class DataSeeder
{
	private record CategoryDto(string Name, string Description);
	private record ItemDto(string Name, string Description, decimal Price);

	public static async Task SeedDataAsync(ICategoryService categoryService, IItemService itemService)
	{
		var categoriesJson = await File.ReadAllTextAsync("Data/SeedData/categories.json");
		var itemsJson = await File.ReadAllTextAsync("Data/SeedData/items.json");

		var categories = JsonSerializer.Deserialize<List<CategoryDto>>(categoriesJson) ?? [];
		var itemsByCategory = JsonSerializer.Deserialize<Dictionary<string, List<ItemDto>>>(itemsJson) ?? [];

		// Seed Categories
		var categoryMap = new Dictionary<string, Category>();

		foreach (var categoryDto in categories)
		{
			var category = await categoryService.CreateCategoryAsync(new Category
			{
				Name = categoryDto.Name,
				Description = categoryDto.Description
			});
			categoryMap[categoryDto.Name] = category;
		}

		// Seed Items by Category
		foreach (var (categoryName, items) in itemsByCategory)
		{
			if (categoryMap.TryGetValue(categoryName, out var category))
			{
				foreach (var itemDto in items)
				{
					await itemService.CreateItemAsync(new Item
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
