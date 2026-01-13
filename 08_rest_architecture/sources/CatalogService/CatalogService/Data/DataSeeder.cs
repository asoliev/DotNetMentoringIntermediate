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

	private static readonly List<(string Name, string Description, decimal Price)> ElectronicsItems =
	[
		("Laptop Pro 15", "High-performance laptop with 16GB RAM and 512GB SSD", 1299.99m),
		("Wireless Mouse", "Ergonomic wireless mouse with USB receiver", 29.99m),
		("Smartphone X", "Latest smartphone with 5G connectivity", 899.99m),
		("Bluetooth Headphones", "Noise-cancelling over-ear headphones", 149.99m),
		("USB-C Hub", "7-in-1 USB-C hub with HDMI and card reader", 49.99m),
		("4K Monitor", "27-inch 4K UHD monitor with HDR support", 399.99m),
		("Mechanical Keyboard", "RGB backlit mechanical keyboard with blue switches", 129.99m)
	];

	private static readonly List<(string Name, string Description, decimal Price)> BooksItems =
	[
		("Clean Code", "A Handbook of Agile Software Craftsmanship by Robert C. Martin", 42.99m),
		("Design Patterns", "Elements of Reusable Object-Oriented Software", 54.99m),
		("The Pragmatic Programmer", "Your Journey to Mastery, 20th Anniversary Edition", 39.99m),
		("Domain-Driven Design", "Tackling Complexity in the Heart of Software", 49.99m),
		("Refactoring", "Improving the Design of Existing Code", 44.99m),
		("Code Complete", "A Practical Handbook of Software Construction", 52.99m)
	];

	private static readonly List<(string Name, string Description, decimal Price)> ClothingItems =
	[
		("Cotton T-Shirt", "Comfortable 100% cotton t-shirt, available in multiple colors", 19.99m),
		("Denim Jeans", "Classic fit denim jeans", 59.99m),
		("Winter Jacket", "Warm insulated jacket for cold weather", 129.99m),
		("Running Shoes", "Lightweight running shoes with cushioned sole", 89.99m),
		("Hoodie", "Pullover hoodie with kangaroo pocket", 44.99m),
		("Baseball Cap", "Adjustable baseball cap with embroidered logo", 24.99m)
	];

	private static readonly List<(string Name, string Description, decimal Price)> HomeGardenItems =
	[
		("LED Desk Lamp", "Adjustable LED lamp with touch controls", 34.99m),
		("Garden Tool Set", "Complete 10-piece garden tool set with carrying bag", 79.99m),
		("Throw Pillow Set", "Set of 4 decorative throw pillows", 44.99m),
		("Coffee Maker", "Programmable coffee maker with thermal carafe", 69.99m),
		("Indoor Plant Pot", "Ceramic plant pot with drainage tray", 19.99m),
		("Vacuum Cleaner", "Cordless stick vacuum with multiple attachments", 199.99m)
	];

	private static readonly List<(string Name, string Description, decimal Price)> SportsOutdoorsItems =
	[
		("Yoga Mat", "Non-slip yoga mat with carrying strap", 24.99m),
		("Camping Tent", "4-person waterproof camping tent", 159.99m),
		("Basketball", "Official size basketball", 29.99m),
		("Dumbbell Set", "Adjustable dumbbell set 5-50 lbs", 299.99m),
		("Hiking Backpack", "40L hiking backpack with rain cover", 89.99m)
	];

	private static readonly List<(string Name, string Description, decimal Price)> ToysGamesItems =
	[
		("Building Blocks Set", "500-piece creative building blocks", 39.99m),
		("Board Game Collection", "Classic board game with family-friendly rules", 34.99m),
		("RC Car", "Remote control racing car with rechargeable battery", 79.99m),
		("Puzzle Set", "1000-piece jigsaw puzzle", 19.99m),
		("Action Figure", "Collectible action figure with accessories", 24.99m)
	];

	private static readonly Dictionary<string, List<(string Name, string Description, decimal Price)>> ItemsByCategory = new()
	{
		{ "Electronics", ElectronicsItems },
		{ "Books", BooksItems },
		{ "Clothing", ClothingItems },
		{ "Home & Garden", HomeGardenItems },
		{ "Sports & Outdoors", SportsOutdoorsItems },
		{ "Toys & Games", ToysGamesItems }
	};

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

		// Seed Items by Category
		foreach (var (categoryName, items) in ItemsByCategory)
		{
			if (categoryMap.TryGetValue(categoryName, out var category))
			{
				foreach (var (name, description, price) in items)
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
}
