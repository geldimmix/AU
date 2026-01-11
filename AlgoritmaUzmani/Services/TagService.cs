using AlgoritmaUzmani.Data;
using AlgoritmaUzmani.Helpers;
using AlgoritmaUzmani.Models.Entities;
using AlgoritmaUzmani.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlgoritmaUzmani.Services;

public class TagService : ITagService
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cache;
    private readonly ITranslationService _translationService;
    private readonly ILogger<TagService> _logger;
    private const string CachePrefix = "tag_";

    public TagService(
        ApplicationDbContext context, 
        ICacheService cache,
        ITranslationService translationService,
        ILogger<TagService> logger)
    {
        _context = context;
        _cache = cache;
        _translationService = translationService;
        _logger = logger;
    }

    public async Task<List<Tag>> GetAllAsync()
    {
        var cacheKey = $"{CachePrefix}all";
        var cached = await _cache.GetAsync<List<Tag>>(cacheKey);
        if (cached != null) return cached;

        var tags = await _context.Tags.OrderBy(t => t.NameTr).ToListAsync();
        await _cache.SetAsync(cacheKey, tags, TimeSpan.FromHours(1));
        return tags;
    }

    public async Task<Tag?> GetByIdAsync(int id)
    {
        return await _context.Tags.FindAsync(id);
    }

    public async Task<Tag?> GetBySlugAsync(string slug, string language = "tr")
    {
        if (language == "en")
            return await _context.Tags.FirstOrDefaultAsync(t => t.SlugEn == slug);
        return await _context.Tags.FirstOrDefaultAsync(t => t.SlugTr == slug);
    }

    public async Task<Tag> CreateAsync(Tag tag)
    {
        tag.SlugTr = SlugHelper.GenerateSlug(tag.NameTr);
        tag.CreatedAt = DateTime.UtcNow;

        // Auto-translate to English if not provided
        if (string.IsNullOrEmpty(tag.NameEn))
        {
            try
            {
                tag.NameEn = await _translationService.TranslateToEnglishAsync(tag.NameTr);
                _logger.LogInformation("Tag translated: {NameTr} -> {NameEn}", tag.NameTr, tag.NameEn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to translate tag: {NameTr}", tag.NameTr);
            }
        }

        if (!string.IsNullOrEmpty(tag.NameEn))
            tag.SlugEn = SlugHelper.GenerateSlug(tag.NameEn);

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        await _cache.RemoveByPrefixAsync(CachePrefix);
        return tag;
    }

    public async Task<Tag> UpdateAsync(Tag tag)
    {
        var existing = await _context.Tags.FindAsync(tag.Id);
        if (existing == null)
            throw new InvalidOperationException("Tag not found");

        bool needsTranslation = existing.NameTr != tag.NameTr;

        existing.NameTr = tag.NameTr;
        existing.SlugTr = SlugHelper.GenerateSlug(tag.NameTr);
        existing.Color = tag.Color;

        // Auto-translate if Turkish name changed
        if (needsTranslation)
        {
            try
            {
                existing.NameEn = await _translationService.TranslateToEnglishAsync(tag.NameTr);
                _logger.LogInformation("Tag updated and translated: {NameTr} -> {NameEn}", tag.NameTr, existing.NameEn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to translate tag: {NameTr}", tag.NameTr);
                existing.NameEn = tag.NameEn;
            }
        }
        else
        {
            existing.NameEn = tag.NameEn;
        }

        existing.SlugEn = !string.IsNullOrEmpty(existing.NameEn) 
            ? SlugHelper.GenerateSlug(existing.NameEn) 
            : null;

        await _context.SaveChangesAsync();

        await _cache.RemoveByPrefixAsync(CachePrefix);
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag == null) return false;

        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();

        await _cache.RemoveByPrefixAsync(CachePrefix);
        return true;
    }
}

public class SeoTagService : ISeoTagService
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cache;
    private readonly ITranslationService _translationService;
    private readonly ILogger<SeoTagService> _logger;
    private const string CachePrefix = "seotag_";

    public SeoTagService(
        ApplicationDbContext context, 
        ICacheService cache,
        ITranslationService translationService,
        ILogger<SeoTagService> logger)
    {
        _context = context;
        _cache = cache;
        _translationService = translationService;
        _logger = logger;
    }

    public async Task<List<SeoTag>> GetAllAsync()
    {
        var cacheKey = $"{CachePrefix}all";
        var cached = await _cache.GetAsync<List<SeoTag>>(cacheKey);
        if (cached != null) return cached;

        var tags = await _context.SeoTags.OrderBy(t => t.NameTr).ToListAsync();
        await _cache.SetAsync(cacheKey, tags, TimeSpan.FromHours(1));
        return tags;
    }

    public async Task<SeoTag?> GetByIdAsync(int id)
    {
        return await _context.SeoTags.FindAsync(id);
    }

    public async Task<SeoTag> CreateAsync(SeoTag seoTag)
    {
        seoTag.CreatedAt = DateTime.UtcNow;

        // Auto-translate to English if not provided
        if (string.IsNullOrEmpty(seoTag.NameEn))
        {
            try
            {
                seoTag.NameEn = await _translationService.TranslateToEnglishAsync(seoTag.NameTr);
                _logger.LogInformation("SeoTag translated: {NameTr} -> {NameEn}", seoTag.NameTr, seoTag.NameEn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to translate SeoTag: {NameTr}", seoTag.NameTr);
            }
        }

        _context.SeoTags.Add(seoTag);
        await _context.SaveChangesAsync();

        await _cache.RemoveByPrefixAsync(CachePrefix);
        return seoTag;
    }

    public async Task<SeoTag> UpdateAsync(SeoTag seoTag)
    {
        var existing = await _context.SeoTags.FindAsync(seoTag.Id);
        if (existing == null)
            throw new InvalidOperationException("SeoTag not found");

        bool needsTranslation = existing.NameTr != seoTag.NameTr;

        existing.NameTr = seoTag.NameTr;

        // Auto-translate if Turkish name changed
        if (needsTranslation)
        {
            try
            {
                existing.NameEn = await _translationService.TranslateToEnglishAsync(seoTag.NameTr);
                _logger.LogInformation("SeoTag updated and translated: {NameTr} -> {NameEn}", seoTag.NameTr, existing.NameEn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to translate SeoTag: {NameTr}", seoTag.NameTr);
                existing.NameEn = seoTag.NameEn;
            }
        }
        else
        {
            existing.NameEn = seoTag.NameEn;
        }

        await _context.SaveChangesAsync();

        await _cache.RemoveByPrefixAsync(CachePrefix);
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var tag = await _context.SeoTags.FindAsync(id);
        if (tag == null) return false;

        _context.SeoTags.Remove(tag);
        await _context.SaveChangesAsync();

        await _cache.RemoveByPrefixAsync(CachePrefix);
        return true;
    }
}

