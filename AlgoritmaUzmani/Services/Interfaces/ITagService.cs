using AlgoritmaUzmani.Models.Entities;

namespace AlgoritmaUzmani.Services.Interfaces;

public interface ITagService
{
    Task<List<Tag>> GetAllAsync();
    Task<Tag?> GetByIdAsync(int id);
    Task<Tag?> GetBySlugAsync(string slug, string language = "tr");
    Task<Tag> CreateAsync(Tag tag);
    Task<Tag> UpdateAsync(Tag tag);
    Task<bool> DeleteAsync(int id);
}

public interface ISeoTagService
{
    Task<List<SeoTag>> GetAllAsync();
    Task<SeoTag?> GetByIdAsync(int id);
    Task<SeoTag> CreateAsync(SeoTag seoTag);
    Task<SeoTag> UpdateAsync(SeoTag seoTag);
    Task<bool> DeleteAsync(int id);
}



