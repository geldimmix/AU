using AlgoritmaUzmani.Data;
using AlgoritmaUzmani.Models.Entities;
using AlgoritmaUzmani.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlgoritmaUzmani.Services;

public class SiteSettingService : ISiteSettingService
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cache;
    private const string CachePrefix = "sitesetting_";

    public SiteSettingService(ApplicationDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<SiteSetting>> GetAllAsync()
    {
        var cacheKey = $"{CachePrefix}all";
        var cached = await _cache.GetAsync<List<SiteSetting>>(cacheKey);
        if (cached != null) return cached;

        var settings = await _context.SiteSettings
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Key)
            .ToListAsync();

        await _cache.SetAsync(cacheKey, settings, TimeSpan.FromHours(1));
        return settings;
    }

    public async Task<List<SiteSetting>> GetByCategoryAsync(string category)
    {
        return await _context.SiteSettings
            .Where(s => s.Category == category && s.IsActive)
            .OrderBy(s => s.Key)
            .ToListAsync();
    }

    public async Task<SiteSetting?> GetByKeyAsync(string key)
    {
        var cacheKey = $"{CachePrefix}{key}";
        var cached = await _cache.GetAsync<SiteSetting>(cacheKey);
        if (cached != null) return cached;

        var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);

        if (setting != null)
            await _cache.SetAsync(cacheKey, setting, TimeSpan.FromHours(1));

        return setting;
    }

    public async Task<string?> GetValueAsync(string key)
    {
        var setting = await GetByKeyAsync(key);
        return setting?.IsActive == true ? setting.Value : null;
    }

    public async Task<SiteSetting> CreateOrUpdateAsync(string key, string? value, string? description = null, string? category = null)
    {
        var existing = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);

        if (existing == null)
        {
            existing = new SiteSetting
            {
                Key = key,
                Value = value,
                Description = description,
                Category = category,
                CreatedAt = DateTime.UtcNow
            };
            _context.SiteSettings.Add(existing);
        }
        else
        {
            existing.Value = value;
            if (description != null) existing.Description = description;
            if (category != null) existing.Category = category;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        await _cache.RemoveByPrefixAsync(CachePrefix);

        return existing;
    }

    public async Task<bool> DeleteAsync(string key)
    {
        var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting == null) return false;

        _context.SiteSettings.Remove(setting);
        await _context.SaveChangesAsync();
        await _cache.RemoveByPrefixAsync(CachePrefix);

        return true;
    }

    public async Task<string> GetHeaderScriptsAsync()
    {
        var setting = await GetByKeyAsync("header_scripts");
        return setting?.IsActive == true ? setting.Value ?? "" : "";
    }

    public async Task<string> GetFooterScriptsAsync()
    {
        var setting = await GetByKeyAsync("footer_scripts");
        return setting?.IsActive == true ? setting.Value ?? "" : "";
    }
}

