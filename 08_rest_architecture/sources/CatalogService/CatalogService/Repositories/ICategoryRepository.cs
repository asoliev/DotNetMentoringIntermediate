using CatalogService.Models;

namespace CatalogService.Repositories;

/// <summary>
/// Repository interface for Category data access
/// </summary>
public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<Category> AddAsync(Category category);
    Task<Category?> UpdateAsync(int id, Category category);
    Task<bool> DeleteAsync(int id);
}
