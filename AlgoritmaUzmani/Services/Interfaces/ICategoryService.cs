using AlgoritmaUzmani.Models.Entities;

namespace AlgoritmaUzmani.Services.Interfaces;

public interface ICategoryService
{
    Task<List<Category>> GetAllAsync(bool activeOnly = true);
    Task<Category?> GetByIdAsync(int id);
    Task<Category?> GetBySlugAsync(string slug, string language = "tr");
    Task<Category> CreateAsync(Category category);
    Task<Category> UpdateAsync(Category category);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

