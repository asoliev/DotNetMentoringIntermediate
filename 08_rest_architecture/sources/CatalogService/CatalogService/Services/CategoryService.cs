using CatalogService.Models;
using CatalogService.Repositories;

namespace CatalogService.Services;

public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        return categoryRepository.GetAllAsync();
    }

    public Task<Category?> GetCategoryByIdAsync(int id)
    {
        return categoryRepository.GetByIdAsync(id);
    }

    public Task<Category> CreateCategoryAsync(Category category)
    {
        return categoryRepository.AddAsync(category);
    }

    public Task<Category?> UpdateCategoryAsync(int id, Category category)
    {
        return categoryRepository.UpdateAsync(id, category);
    }

    public Task<bool> DeleteCategoryAsync(int id)
    {
        return categoryRepository.DeleteAsync(id);
    }
}