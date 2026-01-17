using AlgoritmaUzmani.Models.Entities;

namespace AlgoritmaUzmani.Services.Interfaces;

public interface IStaticPageService
{
    Task<List<StaticPage>> GetAllAsync();
    Task<StaticPage?> GetByIdAsync(int id);
    Task<StaticPage?> GetBySlugAsync(string slug);
    Task<StaticPage> UpdateAsync(StaticPage page);
}



