using AlgoritmaUzmani.Models.Entities;

namespace AlgoritmaUzmani.Services.Interfaces;

public interface IGuideService
{
    Task<List<Guide>> GetAllAsync(bool activeOnly = true);
    Task<List<Guide>> GetByCategoryAsync(int categoryId, bool activeOnly = true);
    Task<List<Guide>> GetFeaturedAsync(int count = 5);
    Task<List<Guide>> GetRecentAsync(int count = 10);
    Task<Guide?> GetByIdAsync(int id);
    Task<Guide?> GetBySlugAsync(string slug, string language = "tr");
    Task<Guide?> GetByIdWithRelationsAsync(int id);
    Task<List<Guide>> GetRelatedGuidesAsync(int guideId);
    Task<Guide> CreateAsync(Guide guide);
    Task<Guide> UpdateAsync(Guide guide);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task IncrementViewCountAsync(int id);
    Task SetRelatedGuidesAsync(int guideId, List<int> relatedGuideIds);
    Task SetTagsAsync(int guideId, List<int> tagIds);
    Task SetSeoTagsAsync(int guideId, List<int> seoTagIds);
    Task<List<Guide>> SearchAsync(string query, string language = "tr", int limit = 10);
}

