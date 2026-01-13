using CatalogService.Models;
using CatalogService.Services;

namespace CatalogService.Data;

public static class DataSeeder
{
	private static readonly List<(string Name, string Description)> CategoryData =
	[
		("Electronics", "Electronic devices and gadgets"),
		("Books", "Physical and digital books"),
		("Clothing", "Apparel and fashion items"),
		("Home & Garden", "Home improvement and garden supplies"),
		("Sports & Outdoors", "Sports equipment and outdoor gear"),
		("Toys & Games", "Toys, games, and entertainment")
	];

	private static readonly List<(string Name, string Description, decimal Price, string CategoryName)> ItemData =
	[
        // Electronics
        ("Laptop Pro 15", "High-performance laptop with 16GB RAM and 512GB SSD", 1299.99m, "Electronics"),
		("Wireless Mouse", "Ergonomic wireless mouse with USB receiver", 29.99m, "Electronics"),
		("Smartphone X", "Latest smartphone with 5G connectivity", 899.99m, "Electronics"),
		("Bluetooth Headphones", "Noise-cancelling over-ear headphones", 149.99m, "Electronics"),
		("USB-C Hub", "7-in-1 USB-C hub with HDMI and card reader", 49.99m, "Electronics"),
        
        // Books
        ("Clean Code", "A Handbook of Agile Software Craftsmanship by Robert C. Martin", 42.99m, "Books"),
		("Design Patterns", "Elements of Reusable Object-Oriented Software", 54.99m, "Books"),
		("The Pragmatic Programmer", "Your Journey to Mastery, 20th Anniversary Edition", 39.99m, "Books"),
		("Domain-Driven Design", "Tackling Complexity in the Heart of Software", 49.99m, "Books"),
        
        // Clothing
        ("Cotton T-Shirt", "Comfortable 100% cotton t-shirt, available in multiple colors", 19.99m, "Clothing"),
		("Denim Jeans", "Classic fit denim jeans", 59.99m, "Clothing"),
		("Winter Jacket", "Warm insulated jacket for cold weather", 129.99m, "Clothing"),
		("Running Shoes", "Lightweight running shoes with cushioned sole", 89.99m, "Clothing"),
        
        // Home & Garden
        ("LED Desk Lamp", "Adjustable LED lamp with touch controls", 34.99m, "Home & Garden"),
		("Garden Tool Set", "Complete 10-piece garden tool set with carrying bag", 79.99m, "Home & Garden"),
		("Throw Pillow Set", "Set of 4 decorative throw pillows", 44.99m, "Home & Garden"),
		("Coffee Maker", "Programmable coffee maker with thermal carafe", 69.99m, "Home & Garden"),
        
        // Sports & Outdoors
        ("Yoga Mat", "Non-slip yoga mat with carrying strap", 24.99m, "Sports & Outdoors"),
		("Camping Tent", "4-person waterproof camping tent", 159.99m, "Sports & Outdoors"),
		("Basketball", "Official size basketball", 29.99m, "Sports & Outdoors"),
        
        // Toys & Games
        ("Building Blocks Set", "500-piece creative building blocks", 39.99m, "Toys & Games"),
		("Board Game Collection", "Classic board game with family-friendly rules", 34.99m, "Toys & Games"),
		("RC Car", "Remote control racing car with rechargeable battery", 79.99m, "Toys & Games")
	];

	public static async Task SeedDataAsync(ICategoryService categoryService, IItemService itemService)
	{
		// Seed Categories
		var categoryMap = new Dictionary<string, Category>();

		foreach (var (name, description) in CategoryData)
		{
			var category = await categoryService.CreateCategoryAsync(new Category
			{
				Name = name,
				Description = description
			});
			categoryMap[name] = category;
		}

		// Seed Items
		foreach (var (name, description, price, categoryName) in ItemData)
		{
			if (categoryMap.TryGetValue(categoryName, out var category))
			{
				await itemService.CreateItemAsync(new Item
				{
					Name = name,
					Description = description,
					Price = price,
					CategoryId = category.Id
				});
			}
		}
	}
}
