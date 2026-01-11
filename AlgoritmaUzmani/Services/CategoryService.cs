using AlgoritmaUzmani.Data;
using AlgoritmaUzmani.Helpers;
using AlgoritmaUzmani.Models.Entities;
using AlgoritmaUzmani.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlgoritmaUzmani.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cache;
    private readonly ITranslationService _translationService;
    private readonly ILogger<CategoryService> _logger;
    private const string CachePrefix = "category_";

    public CategoryService(
        ApplicationDbContext context, 
        ICacheService cache,
        ITranslationService translationService,
        ILogger<CategoryService> logger)
    {
        _context = context;
        _cache = cache;
        _translationService = translationService;
        _logger = logger;
    }

    public async Task<List<Category>> GetAllAsync(bool activeOnly = true)
    {
        var cacheKey = $"{CachePrefix}all_{activeOnly}";
        var cached = await _cache.GetAsync<List<Category>>(cacheKey);
        if (cached != null) return cached;

        var query = _context.Categories
            .Include(c => c.Guides)
            .AsQueryable();
        
        if (activeOnly)
            query = query.Where(c => c.IsActive);

        var categories = await query
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        await _cache.SetAsync(cacheKey, categories, TimeSpan.FromHours(1));
        return categories;
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        var cacheKey = $"{CachePrefix}{id}";
        var cached = await _cache.GetAsync<Category>(cacheKey);
        if (cached != null) return cached;

        var category = await _context.Categories
            .Include(c => c.Guides.Where(g => g.IsActive))
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category != null)
            await _cache.SetAsync(cacheKey, category, TimeSpan.FromHours(1));

        return category;
    }

    public async Task<Category?> GetBySlugAsync(string slug, string language = "tr")
    {
        var cacheKey = $"{CachePrefix}slug_{slug}_{language}";
        var cached = await _cache.GetAsync<Category>(cacheKey);
        if (cached != null) return cached;

        Category? category;
        if (language == "en")
        {
            category = await _context.Categories
                .Include(c => c.Guides.Where(g => g.IsActive))
                .FirstOrDefaultAsync(c => c.SlugEn == slug && c.IsActive);
        }
        else
        {
            category = await _context.Categories
                .Include(c => c.Guides.Where(g => g.IsActive))
                .FirstOrDefaultAsync(c => c.SlugTr == slug && c.IsActive);
        }

        if (category != null)
            await _cache.SetAsync(cacheKey, category, TimeSpan.FromHours(1));

        return category;
    }

    public async Task<Category> CreateAsync(Category category)
    {
        category.SlugTr = SlugHelper.GenerateSlug(category.NameTr);
        category.CreatedAt = DateTime.UtcNow;

        // Auto-translate to English
        try
        {
            var nameEnTask = _translationService.TranslateToEnglishAsync(category.NameTr);
            var descEnTask = !string.IsNullOrEmpty(category.DescriptionTr) 
                ? _translationService.TranslateToEnglishAsync(category.DescriptionTr) 
                : Task.FromResult<string>(null!);

            await Task.WhenAll(nameEnTask, descEnTask);

            category.NameEn = await nameEnTask;
            category.DescriptionEn = await descEnTask;
            category.SlugEn = SlugHelper.GenerateSlug(category.NameEn);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to auto-translate category: {Name}", category.NameTr);
            // Continue without translation
        }

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        await _cache.RemoveByPrefixAsync(CachePrefix);
        return category;
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        var existing = await _context.Categories.FindAsync(category.Id);
        if (existing == null)
            throw new InvalidOperationException("Category not found");

        // Check if Turkish content changed - re-translate
        bool needsTranslation = existing.NameTr != category.NameTr || 
                                existing.DescriptionTr != category.DescriptionTr;

        existing.NameTr = category.NameTr;
        existing.SlugTr = SlugHelper.GenerateSlug(category.NameTr);
        existing.DescriptionTr = category.DescriptionTr;
        existing.Icon = category.Icon;
        existing.DisplayOrder = category.DisplayOrder;
        existing.IsActive = category.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        // Auto-translate if content changed
        if (needsTranslation)
        {
            try
            {
                var nameEnTask = _translationService.TranslateToEnglishAsync(category.NameTr);
                var descEnTask = !string.IsNullOrEmpty(category.DescriptionTr) 
                    ? _translationService.TranslateToEnglishAsync(category.DescriptionTr) 
                    : Task.FromResult<string>(null!);

                await Task.WhenAll(nameEnTask, descEnTask);

                existing.NameEn = await nameEnTask;
                existing.DescriptionEn = await descEnTask;
                existing.SlugEn = SlugHelper.GenerateSlug(existing.NameEn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to auto-translate category: {Name}", category.NameTr);
            }
        }

        await _context.SaveChangesAsync();

        await _cache.RemoveByPrefixAsync(CachePrefix);
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return false;

        // Check if there are guides in this category
        var hasGuides = await _context.Guides.AnyAsync(g => g.CategoryId == id);
        if (hasGuides)
            throw new InvalidOperationException("Bu kategoride rehberler var. Önce rehberleri silin veya taşıyın.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        await _cache.RemoveByPrefixAsync(CachePrefix);
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Categories.AnyAsync(c => c.Id == id);
    }
}
