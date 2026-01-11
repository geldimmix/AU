using AlgoritmaUzmani.Data;
using AlgoritmaUzmani.Helpers;
using AlgoritmaUzmani.Models.Entities;
using AlgoritmaUzmani.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlgoritmaUzmani.Services;

public class GuideService : IGuideService
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cache;
    private const string CachePrefix = "guide_";

    public GuideService(ApplicationDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<Guide>> GetAllAsync(bool activeOnly = true)
    {
        var cacheKey = $"{CachePrefix}all_{activeOnly}";
        var cached = await _cache.GetAsync<List<Guide>>(cacheKey);
        if (cached != null) return cached;

        var query = _context.Guides
            .Include(g => g.Category)
            .Include(g => g.GuideTags)
                .ThenInclude(gt => gt.Tag)
            .AsQueryable();

        if (activeOnly)
            query = query.Where(g => g.IsActive);

        var guides = await query
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        await _cache.SetAsync(cacheKey, guides, TimeSpan.FromMinutes(30));
        return guides;
    }

    public async Task<List<Guide>> GetByCategoryAsync(int categoryId, bool activeOnly = true)
    {
        var cacheKey = $"{CachePrefix}category_{categoryId}_{activeOnly}";
        var cached = await _cache.GetAsync<List<Guide>>(cacheKey);
        if (cached != null) return cached;

        var query = _context.Guides
            .Include(g => g.Category)
            .Include(g => g.GuideTags)
                .ThenInclude(gt => gt.Tag)
            .Where(g => g.CategoryId == categoryId);

        if (activeOnly)
            query = query.Where(g => g.IsActive);

        var guides = await query
            .OrderBy(g => g.DisplayOrder)
            .ThenByDescending(g => g.CreatedAt)
            .ToListAsync();

        await _cache.SetAsync(cacheKey, guides, TimeSpan.FromMinutes(30));
        return guides;
    }

    public async Task<List<Guide>> GetFeaturedAsync(int count = 5)
    {
        var cacheKey = $"{CachePrefix}featured_{count}";
        var cached = await _cache.GetAsync<List<Guide>>(cacheKey);
        if (cached != null) return cached;

        var guides = await _context.Guides
            .Include(g => g.Category)
            .Include(g => g.GuideTags)
                .ThenInclude(gt => gt.Tag)
            .Where(g => g.IsActive && g.IsFeatured)
            .OrderByDescending(g => g.CreatedAt)
            .Take(count)
            .ToListAsync();

        await _cache.SetAsync(cacheKey, guides, TimeSpan.FromMinutes(30));
        return guides;
    }

    public async Task<List<Guide>> GetRecentAsync(int count = 10)
    {
        var cacheKey = $"{CachePrefix}recent_{count}";
        var cached = await _cache.GetAsync<List<Guide>>(cacheKey);
        if (cached != null) return cached;

        var guides = await _context.Guides
            .Include(g => g.Category)
            .Include(g => g.GuideTags)
                .ThenInclude(gt => gt.Tag)
            .Where(g => g.IsActive)
            .OrderByDescending(g => g.CreatedAt)
            .Take(count)
            .ToListAsync();

        await _cache.SetAsync(cacheKey, guides, TimeSpan.FromMinutes(30));
        return guides;
    }

    public async Task<Guide?> GetByIdAsync(int id)
    {
        return await _context.Guides
            .Include(g => g.Category)
            .Include(g => g.GuideTags)
                .ThenInclude(gt => gt.Tag)
            .Include(g => g.GuideSeoTags)
                .ThenInclude(gs => gs.SeoTag)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<Guide?> GetBySlugAsync(string slug, string language = "tr")
    {
        var cacheKey = $"{CachePrefix}slug_{slug}_{language}";
        var cached = await _cache.GetAsync<Guide>(cacheKey);
        if (cached != null) return cached;

        Guide? guide;
        if (language == "en")
        {
            guide = await _context.Guides
                .Include(g => g.Category)
                .Include(g => g.GuideTags)
                    .ThenInclude(gt => gt.Tag)
                .Include(g => g.GuideSeoTags)
                    .ThenInclude(gs => gs.SeoTag)
                .Include(g => g.RelatedGuides)
                    .ThenInclude(rg => rg.Related)
                .FirstOrDefaultAsync(g => g.SlugEn == slug && g.IsActive);
        }
        else
        {
            guide = await _context.Guides
                .Include(g => g.Category)
                .Include(g => g.GuideTags)
                    .ThenInclude(gt => gt.Tag)
                .Include(g => g.GuideSeoTags)
                    .ThenInclude(gs => gs.SeoTag)
                .Include(g => g.RelatedGuides)
                    .ThenInclude(rg => rg.Related)
                .FirstOrDefaultAsync(g => g.SlugTr == slug && g.IsActive);
        }

        if (guide != null)
            await _cache.SetAsync(cacheKey, guide, TimeSpan.FromMinutes(30));

        return guide;
    }

    public async Task<Guide?> GetByIdWithRelationsAsync(int id)
    {
        return await _context.Guides
            .Include(g => g.Category)
            .Include(g => g.GuideTags)
                .ThenInclude(gt => gt.Tag)
            .Include(g => g.GuideSeoTags)
                .ThenInclude(gs => gs.SeoTag)
            .Include(g => g.RelatedGuides)
                .ThenInclude(rg => rg.Related)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<List<Guide>> GetRelatedGuidesAsync(int guideId)
    {
        var cacheKey = $"{CachePrefix}related_{guideId}";
        var cached = await _cache.GetAsync<List<Guide>>(cacheKey);
        if (cached != null) return cached;

        var relatedIds = await _context.RelatedGuides
            .Where(rg => rg.GuideId == guideId)
            .OrderBy(rg => rg.DisplayOrder)
            .Select(rg => rg.RelatedGuideId)
            .ToListAsync();

        var guides = await _context.Guides
            .Include(g => g.Category)
            .Include(g => g.GuideTags)
                .ThenInclude(gt => gt.Tag)
            .Where(g => relatedIds.Contains(g.Id) && g.IsActive)
            .ToListAsync();

        // Preserve order
        var orderedGuides = relatedIds
            .Select(id => guides.FirstOrDefault(g => g.Id == id))
            .Where(g => g != null)
            .Cast<Guide>()
            .ToList();

        await _cache.SetAsync(cacheKey, orderedGuides, TimeSpan.FromMinutes(30));
        return orderedGuides;
    }

    public async Task<Guide> CreateAsync(Guide guide)
    {
        guide.SlugTr = SlugHelper.GenerateSlug(guide.TitleTr);
        if (!string.IsNullOrEmpty(guide.TitleEn))
            guide.SlugEn = SlugHelper.GenerateSlug(guide.TitleEn);

        // Ensure MetaDescription fields don't exceed 160 chars
        guide.MetaDescriptionTr = TruncateString(guide.MetaDescriptionTr, 160);
        guide.MetaDescriptionEn = TruncateString(guide.MetaDescriptionEn, 160);

        guide.CreatedAt = DateTime.UtcNow;

        _context.Guides.Add(guide);
        await _context.SaveChangesAsync();

        await _cache.RemoveByPrefixAsync(CachePrefix);
        await _cache.RemoveByPrefixAsync("category_");
        return guide;
    }

    public async Task<Guide> UpdateAsync(Guide guide)
    {
        var existing = await _context.Guides.FindAsync(guide.Id);
        if (existing == null)
            throw new InvalidOperationException("Guide not found");

        existing.CategoryId = guide.CategoryId;
        existing.TitleTr = guide.TitleTr;
        existing.TitleEn = guide.TitleEn;
        existing.SlugTr = SlugHelper.GenerateSlug(guide.TitleTr);
        existing.SlugEn = !string.IsNullOrEmpty(guide.TitleEn) 
            ? SlugHelper.GenerateSlug(guide.TitleEn) 
            : null;
        existing.SummaryTr = guide.SummaryTr;
        existing.SummaryEn = guide.SummaryEn;
        existing.ContentTr = guide.ContentTr;
        existing.ContentEn = guide.ContentEn;
        existing.MetaDescriptionTr = TruncateString(guide.MetaDescriptionTr, 160);
        existing.MetaDescriptionEn = TruncateString(guide.MetaDescriptionEn, 160);
        existing.SeoKeywordsTr = guide.SeoKeywordsTr;
        existing.SeoKeywordsEn = guide.SeoKeywordsEn;
        existing.FeaturedImage = guide.FeaturedImage;
        existing.FeaturedImageAltTr = guide.FeaturedImageAltTr;
        existing.FeaturedImageAltEn = guide.FeaturedImageAltEn;
        existing.IsFeatured = guide.IsFeatured;
        existing.DisplayOrder = guide.DisplayOrder;
        existing.IsActive = guide.IsActive;
        existing.IsTranslated = guide.IsTranslated;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _cache.RemoveByPrefixAsync(CachePrefix);
        await _cache.RemoveByPrefixAsync("category_");
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var guide = await _context.Guides.FindAsync(id);
        if (guide == null) return false;

        _context.Guides.Remove(guide);
        await _context.SaveChangesAsync();

        await _cache.RemoveByPrefixAsync(CachePrefix);
        await _cache.RemoveByPrefixAsync("category_");
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Guides.AnyAsync(g => g.Id == id);
    }

    public async Task IncrementViewCountAsync(int id)
    {
        await _context.Guides
            .Where(g => g.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(g => g.ViewCount, g => g.ViewCount + 1));
    }

    public async Task SetRelatedGuidesAsync(int guideId, List<int> relatedGuideIds)
    {
        // Remove existing relations
        var existing = await _context.RelatedGuides
            .Where(rg => rg.GuideId == guideId)
            .ToListAsync();
        _context.RelatedGuides.RemoveRange(existing);

        // Add new relations
        var order = 0;
        foreach (var relatedId in relatedGuideIds)
        {
            if (relatedId != guideId) // Can't relate to self
            {
                _context.RelatedGuides.Add(new RelatedGuide
                {
                    GuideId = guideId,
                    RelatedGuideId = relatedId,
                    DisplayOrder = order++
                });
            }
        }

        await _context.SaveChangesAsync();
        await _cache.RemoveByPrefixAsync($"{CachePrefix}related_{guideId}");
    }

    public async Task SetTagsAsync(int guideId, List<int> tagIds)
    {
        var existing = await _context.GuideTags
            .Where(gt => gt.GuideId == guideId)
            .ToListAsync();
        _context.GuideTags.RemoveRange(existing);

        foreach (var tagId in tagIds)
        {
            _context.GuideTags.Add(new GuideTag
            {
                GuideId = guideId,
                TagId = tagId
            });
        }

        await _context.SaveChangesAsync();
        await _cache.RemoveByPrefixAsync(CachePrefix);
    }

    public async Task SetSeoTagsAsync(int guideId, List<int> seoTagIds)
    {
        var existing = await _context.GuideSeoTags
            .Where(gs => gs.GuideId == guideId)
            .ToListAsync();
        _context.GuideSeoTags.RemoveRange(existing);

        foreach (var seoTagId in seoTagIds)
        {
            _context.GuideSeoTags.Add(new GuideSeoTag
            {
                GuideId = guideId,
                SeoTagId = seoTagId
            });
        }

        await _context.SaveChangesAsync();
        await _cache.RemoveByPrefixAsync(CachePrefix);
    }

    public async Task<List<Guide>> SearchAsync(string query, string language = "tr", int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return new List<Guide>();

        query = query.ToLower().Trim();

        var guides = await _context.Guides
            .Include(g => g.Category)
            .Where(g => g.IsActive)
            .Where(g => 
                (language == "tr" 
                    ? g.TitleTr.ToLower().Contains(query) || 
                      (g.SummaryTr != null && g.SummaryTr.ToLower().Contains(query))
                    : (g.TitleEn != null && g.TitleEn.ToLower().Contains(query)) || 
                      (g.SummaryEn != null && g.SummaryEn.ToLower().Contains(query)) ||
                      g.TitleTr.ToLower().Contains(query)
                )
            )
            .OrderByDescending(g => 
                language == "tr" 
                    ? g.TitleTr.ToLower().StartsWith(query) 
                    : (g.TitleEn != null && g.TitleEn.ToLower().StartsWith(query))
            )
            .ThenByDescending(g => g.ViewCount)
            .Take(limit)
            .ToListAsync();

        return guides;
    }

    private static string? TruncateString(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        
        value = value.Trim();
        if (value.Length <= maxLength) return value;
        
        return value.Substring(0, maxLength - 3).TrimEnd() + "...";
    }
}

