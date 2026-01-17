using AlgoritmaUzmani.Models.Entities;

namespace AlgoritmaUzmani.Services.Interfaces;

public interface ISiteSettingService
{
    Task<List<SiteSetting>> GetAllAsync();
    Task<List<SiteSetting>> GetByCategoryAsync(string category);
    Task<SiteSetting?> GetByKeyAsync(string key);
    Task<string?> GetValueAsync(string key);
    Task<SiteSetting> CreateOrUpdateAsync(string key, string? value, string? description = null, string? category = null);
    Task<bool> DeleteAsync(string key);
    Task<string> GetHeaderScriptsAsync();
    Task<string> GetFooterScriptsAsync();
}



