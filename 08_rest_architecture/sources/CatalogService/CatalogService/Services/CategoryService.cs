using CatalogService.Models;

namespace CatalogService.Services;

public class CategoryService : ICategoryService
{
    private readonly List<Category> _categories = new();
    private int _nextCategoryId = 1;
    private readonly object _lock = new();

    public Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        lock (_lock)
        {
            return Task.FromResult<IEnumerable<Category>>(_categories.ToList());
        }
    }

    public Task<Category?> GetCategoryByIdAsync(int id)
    {
        lock (_lock)
        {
            return Task.FromResult(_categories.FirstOrDefault(c => c.Id == id));
        }
    }

    public Task<Category> CreateCategoryAsync(Category category)
    {
        lock (_lock)
        {
            category.Id = _nextCategoryId++;
            _categories.Add(category);
            return Task.FromResult(category);
        }
    }

    public Task<Category?> UpdateCategoryAsync(int id, Category category)
    {
        lock (_lock)
        {
            var existingCategory = _categories.FirstOrDefault(c => c.Id == id);
            if (existingCategory == null)
            {
                return Task.FromResult<Category?>(null);
            }

            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;
            return Task.FromResult<Category?>(existingCategory);
        }
    }

    public Task<bool> DeleteCategoryAsync(int id)
    {
        lock (_lock)
        {
            var category = _categories.FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return Task.FromResult(false);
            }

            _categories.Remove(category);
            return Task.FromResult(true);
        }
    }
}
