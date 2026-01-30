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
        // Get or create a lock for this specific category to prevent race conditions
        // This ensures that item creation/update operations on this category wait
        // until the deletion is complete, preventing orphaned items
        var categoryLock = CategoryLockManager.GetLock(id);
        
        await categoryLock.WaitAsync();
        try
        {
            // Verify category exists before proceeding with deletion
            var category = await categoryRepository.GetByIdAsync(id);
            if (category == null)
                return false;

            // Delete all items in this category (cascade delete)
            await itemRepository.DeleteByCategoryIdAsync(id);

            // Double-check: verify category still exists before deleting
            // This prevents race conditions where category might have been deleted
            // by another thread between the first check and item deletion
            var categoryStillExists = await categoryRepository.GetByIdAsync(id);
            if (categoryStillExists == null)
                return false;

            // Delete the category itself
            var deleted = await categoryRepository.DeleteAsync(id);
            
            // Clean up the lock if category was successfully deleted
            if (deleted)
            {
                CategoryLockManager.RemoveLock(id);
            }
            
            return deleted;
        }
        finally
        {
            categoryLock.Release();
        }
    }
}