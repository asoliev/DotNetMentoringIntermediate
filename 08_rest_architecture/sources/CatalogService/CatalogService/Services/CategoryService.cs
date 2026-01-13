using CatalogService.Data;
using CatalogService.Models;

namespace CatalogService.Services;

public class CategoryService(InMemoryDataStore dataStore) : ICategoryService
{
    public Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        var categories = dataStore.GetAllCategories();
        return Task.FromResult(categories);
    }

    public Task<Category?> GetCategoryByIdAsync(int id)
    {
        var category = dataStore.GetCategoryById(id);
        return Task.FromResult(category);
    }

    public Task<Category> CreateCategoryAsync(Category category)
    {
        var created = dataStore.AddCategory(category);
        return Task.FromResult(created);
    }

    public Task<Category?> UpdateCategoryAsync(int id, Category category)
    {
        var updated = dataStore.UpdateCategory(id, category);
        return Task.FromResult(updated);
    }

    public Task<bool> DeleteCategoryAsync(int id)
    {
        var deleted = dataStore.DeleteCategory(id);
        return Task.FromResult(deleted);
    }
}