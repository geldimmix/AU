using AlgoritmaUzmani.Data;
using AlgoritmaUzmani.Models.Entities;
using AlgoritmaUzmani.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlgoritmaUzmani.Services;

public class StaticPageService : IStaticPageService
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cache;
    private const string CachePrefix = "staticpage_";

    public StaticPageService(ApplicationDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<StaticPage>> GetAllAsync()
    {
        return await _context.StaticPages
            .OrderBy(p => p.Slug)
            .ToListAsync();
    }

    public async Task<StaticPage?> GetByIdAsync(int id)
    {
        return await _context.StaticPages.FindAsync(id);
    }

    public async Task<StaticPage?> GetBySlugAsync(string slug)
    {
        var cacheKey = $"{CachePrefix}{slug}";
        var cached = await _cache.GetAsync<StaticPage>(cacheKey);
        if (cached != null) return cached;

        var page = await _context.StaticPages
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);

        if (page != null)
            await _cache.SetAsync(cacheKey, page, TimeSpan.FromHours(1));

        return page;
    }

    public async Task<StaticPage> UpdateAsync(StaticPage page)
    {
        var existing = await _context.StaticPages.FindAsync(page.Id);
        if (existing == null)
            throw new InvalidOperationException("Page not found");

        existing.TitleTr = page.TitleTr;
        existing.TitleEn = page.TitleEn;
        existing.ContentTr = page.ContentTr;
        existing.ContentEn = page.ContentEn;
        existing.MetaDescriptionTr = page.MetaDescriptionTr;
        existing.MetaDescriptionEn = page.MetaDescriptionEn;
        existing.IsActive = page.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _cache.RemoveByPrefixAsync(CachePrefix);

        return existing;
    }
}

