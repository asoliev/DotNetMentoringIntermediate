using CatalogService.Models;
using CatalogService.Repositories;

namespace CatalogService.Services;

public class CategoryService(ICategoryRepository categoryRepository, IItemRepository itemRepository) : ICategoryService
{
    public Task<IEnumerable<Category>> GetCategoriesAsync() => categoryRepository.GetAllAsync();
    public Task<Category?> GetCategoryByIdAsync(int id) => categoryRepository.GetByIdAsync(id);
    public Task<Category> CreateCategoryAsync(Category category) => categoryRepository.AddAsync(category);
    public Task<Category?> UpdateCategoryAsync(int id, Category category) => categoryRepository.UpdateAsync(id, category);

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        // First delete all items in this category (cascade delete)
        await itemRepository.DeleteByCategoryIdAsync(id);

        // Then delete the category itself
        return await categoryRepository.DeleteAsync(id);
    }
}