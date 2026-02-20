using AlgoritmaUzmani.Data;
using AlgoritmaUzmani.Helpers;
using AlgoritmaUzmani.Models.Entities;
using AlgoritmaUzmani.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AlgoritmaUzmani.Services;

public class GuideService : IGuideService
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cache;
    private readonly ITranslationService _translationService;
    private readonly ILogger<GuideService> _logger;
    private const string CachePrefix = "guide_";

    public GuideService(
        ApplicationDbContext context,
        ICacheService cache,
        ITranslationService translationService,
        ILogger<GuideService> logger)
    {
        _context = context;
        _cache = cache;
        _translationService = translationService;
        _logger = logger;
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
            .OrderByDescending(g => g.IsFeatured)
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
            guide = await _context.Guides
                .Include(g => g.Category)
                .Include(g => g.GuideTags)
                    .ThenInclude(gt => gt.Tag)
                .Include(g => g.GuideSeoTags)
                    .ThenInclude(gs => gs.SeoTag)
                .Include(g => g.CodeBlocks)
                .FirstOrDefaultAsync(g => g.SlugEn == slug && g.IsActive);
        else
            guide = await _context.Guides
                .Include(g => g.Category)
                .Include(g => g.GuideTags)
                    .ThenInclude(gt => gt.Tag)
                .Include(g => g.GuideSeoTags)
                    .ThenInclude(gs => gs.SeoTag)
                .Include(g => g.CodeBlocks)
                .FirstOrDefaultAsync(g => g.SlugTr == slug && g.IsActive);

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
            .Include(g => g.CodeBlocks)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<List<Guide>> GetRelatedGuidesAsync(int guideId)
    {
        var relatedIds = await _context.RelatedGuides
            .Where(rg => rg.GuideId == guideId)
            .Select(rg => rg.RelatedGuideId)
            .ToListAsync();

        return await _context.Guides
            .Include(g => g.Category)
            .Where(g => relatedIds.Contains(g.Id) && g.IsActive)
            .ToListAsync();
    }

    public async Task<Guide> CreateAsync(Guide guide)
    {
        guide.SlugTr = SlugHelper.GenerateSlug(guide.TitleTr);
        guide.CreatedAt = DateTime.UtcNow;

        // Auto-translate to English if not provided
        if (string.IsNullOrEmpty(guide.TitleEn))
        {
            try
            {
                guide.TitleEn = await _translationService.TranslateToEnglishAsync(guide.TitleTr);
                if (!string.IsNullOrEmpty(guide.SummaryTr))
                    guide.SummaryEn = await _translationService.TranslateToEnglishAsync(guide.SummaryTr);
                if (!string.IsNullOrEmpty(guide.ContentTr))
                    guide.ContentEn = await _translationService.TranslateToEnglishAsync(guide.ContentTr);
                if (!string.IsNullOrEmpty(guide.MetaDescriptionTr))
                    guide.MetaDescriptionEn = await _translationService.TranslateToEnglishAsync(guide.MetaDescriptionTr);
                guide.IsTranslated = true;
                _logger.LogInformation("Guide translated: {TitleTr} -> {TitleEn}", guide.TitleTr, guide.TitleEn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to translate guide: {TitleTr}", guide.TitleTr);
            }
        }

        if (!string.IsNullOrEmpty(guide.TitleEn))
            guide.SlugEn = SlugHelper.GenerateSlug(guide.TitleEn);

        _context.Guides.Add(guide);
        await _context.SaveChangesAsync();

        await _cache.RemoveByPrefixAsync(CachePrefix);
        return guide;
    }

    public async Task<Guide> UpdateAsync(Guide guide)
    {
        var existing = await _context.Guides.FindAsync(guide.Id);
        if (existing == null)
            throw new InvalidOperationException("Guide not found");

        bool needsTranslation = existing.TitleTr != guide.TitleTr || existing.ContentTr != guide.ContentTr;

        existing.CategoryId = guide.CategoryId;
        existing.TitleTr = guide.TitleTr;
        existing.SlugTr = SlugHelper.GenerateSlug(guide.TitleTr);
        existing.SummaryTr = guide.SummaryTr;
        existing.ContentTr = guide.ContentTr;
        existing.MetaDescriptionTr = guide.MetaDescriptionTr;
        existing.SeoKeywordsTr = guide.SeoKeywordsTr;
        existing.FeaturedImage = guide.FeaturedImage;
        existing.FeaturedImageAltTr = guide.FeaturedImageAltTr;
        existing.IsFeatured = guide.IsFeatured;
        existing.DisplayOrder = guide.DisplayOrder;
        existing.IsActive = guide.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        // Auto-translate if Turkish content changed
        if (needsTranslation && string.IsNullOrEmpty(guide.TitleEn))
        {
            try
            {
                existing.TitleEn = await _translationService.TranslateToEnglishAsync(guide.TitleTr);
                if (!string.IsNullOrEmpty(guide.SummaryTr))
                    existing.SummaryEn = await _translationService.TranslateToEnglishAsync(guide.SummaryTr);
                if (!string.IsNullOrEmpty(guide.ContentTr))
                    existing.ContentEn = await _translationService.TranslateToEnglishAsync(guide.ContentTr);
                if (!string.IsNullOrEmpty(guide.MetaDescriptionTr))
                    existing.MetaDescriptionEn = await _translationService.TranslateToEnglishAsync(guide.MetaDescriptionTr);
                existing.IsTranslated = true;
                _logger.LogInformation("Guide updated and translated: {TitleTr} -> {TitleEn}", guide.TitleTr, existing.TitleEn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to translate guide: {TitleTr}", guide.TitleTr);
                existing.TitleEn = guide.TitleEn;
                existing.SummaryEn = guide.SummaryEn;
                existing.ContentEn = guide.ContentEn;
                existing.MetaDescriptionEn = guide.MetaDescriptionEn;
            }
        }
        else
        {
            existing.TitleEn = guide.TitleEn;
            existing.SummaryEn = guide.SummaryEn;
            existing.ContentEn = guide.ContentEn;
            existing.MetaDescriptionEn = guide.MetaDescriptionEn;
            existing.SeoKeywordsEn = guide.SeoKeywordsEn;
            existing.FeaturedImageAltEn = guide.FeaturedImageAltEn;
        }

        existing.SlugEn = !string.IsNullOrEmpty(existing.TitleEn)
            ? SlugHelper.GenerateSlug(existing.TitleEn)
            : null;

        await _context.SaveChangesAsync();

        await _cache.RemoveByPrefixAsync(CachePrefix);
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var guide = await _context.Guides.FindAsync(id);
        if (guide == null) return false;

        _context.Guides.Remove(guide);
        await _context.SaveChangesAsync();

        // Hem guide hem de category cache'lerini temizle
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
        var guide = await _context.Guides.FindAsync(id);
        if (guide != null)
        {
            guide.ViewCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task SetRelatedGuidesAsync(int guideId, List<int> relatedGuideIds)
    {
        // Remove existing relations
        var existing = await _context.RelatedGuides
            .Where(rg => rg.GuideId == guideId)
            .ToListAsync();
        _context.RelatedGuides.RemoveRange(existing);

        // Add new relations
        foreach (var relatedId in relatedGuideIds)
        {
            _context.RelatedGuides.Add(new RelatedGuide
            {
                GuideId = guideId,
                RelatedGuideId = relatedId
            });
        }

        await _context.SaveChangesAsync();
        await _cache.RemoveByPrefixAsync(CachePrefix);
    }

    public async Task SetTagsAsync(int guideId, List<int> tagIds)
    {
        // Remove existing tags
        var existing = await _context.GuideTags
            .Where(gt => gt.GuideId == guideId)
            .ToListAsync();
        _context.GuideTags.RemoveRange(existing);

        // Add new tags
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
        // Remove existing SEO tags
        var existing = await _context.GuideSeoTags
            .Where(gs => gs.GuideId == guideId)
            .ToListAsync();
        _context.GuideSeoTags.RemoveRange(existing);

        // Add new SEO tags
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
        if (string.IsNullOrWhiteSpace(query))
            return new List<Guide>();

        query = query.ToLower();

        var guides = await _context.Guides
            .Include(g => g.Category)
            .Include(g => g.GuideTags)
                .ThenInclude(gt => gt.Tag)
            .Where(g => g.IsActive)
            .Where(g =>
                (language == "tr" && (
                    g.TitleTr.ToLower().Contains(query) ||
                    (g.SummaryTr != null && g.SummaryTr.ToLower().Contains(query)) ||
                    g.ContentTr.ToLower().Contains(query)
                )) ||
                (language == "en" && (
                    (g.TitleEn != null && g.TitleEn.ToLower().Contains(query)) ||
                    (g.SummaryEn != null && g.SummaryEn.ToLower().Contains(query)) ||
                    (g.ContentEn != null && g.ContentEn.ToLower().Contains(query))
                ))
            )
            .OrderByDescending(g => g.IsFeatured)
            .ThenByDescending(g => g.CreatedAt)
            .Take(limit)
            .ToListAsync();

        return guides;
    }

    public async Task SaveCodeBlocksAsync<T>(int guideId, List<T> codeBlocks) where T : class
    {
        // Remove existing code blocks
        var existing = await _context.CodeBlocks
            .Where(cb => cb.GuideId == guideId)
            .ToListAsync();
        _context.CodeBlocks.RemoveRange(existing);

        // If T is CodeBlock, add directly
        if (typeof(T) == typeof(CodeBlock))
        {
            foreach (var block in codeBlocks.Cast<CodeBlock>())
            {
                block.GuideId = guideId;
                block.CreatedAt = DateTime.UtcNow;
                _context.CodeBlocks.Add(block);
            }
        }
        else
        {
            // Serialize and deserialize if it's a different type
            var json = JsonSerializer.Serialize(codeBlocks);
            var blocks = JsonSerializer.Deserialize<List<CodeBlock>>(json);
            if (blocks != null)
            {
                foreach (var block in blocks)
                {
                    block.GuideId = guideId;
                    block.CreatedAt = DateTime.UtcNow;
                    _context.CodeBlocks.Add(block);
                }
            }
        }

        await _context.SaveChangesAsync();
        await _cache.RemoveByPrefixAsync(CachePrefix);
    }

    public async Task<List<CodeBlock>> GetCodeBlocksByGuideIdAsync(int guideId)
    {
        return await _context.CodeBlocks
            .Where(cb => cb.GuideId == guideId)
            .OrderBy(cb => cb.DisplayOrder)
            .ToListAsync();
    }
}
