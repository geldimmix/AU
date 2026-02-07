using AlgoritmaUzmani.Models.ViewModels.Public;
using AlgoritmaUzmani.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AlgoritmaUzmani.Controllers;

public class GuidesController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly IGuideService _guideService;
    private readonly IVisitorLogService _visitorLogService;
    private readonly ILogger<GuidesController> _logger;

    public GuidesController(
        ICategoryService categoryService,
        IGuideService guideService,
        IVisitorLogService visitorLogService,
        ILogger<GuidesController> logger)
    {
        _categoryService = categoryService;
        _guideService = guideService;
        _visitorLogService = visitorLogService;
        _logger = logger;
    }

    // GET: /rehberler
    [HttpGet("rehberler")]
    public async Task<IActionResult> Index()
    {
        return await GetIndexView("tr");
    }

    // GET: /en/guides
    [HttpGet("en/guides")]
    public async Task<IActionResult> IndexEn()
    {
        return await GetIndexView("en");
    }

    private async Task<IActionResult> GetIndexView(string language)
    {
        var categories = await _categoryService.GetAllAsync(true);
        var featuredGuides = await _guideService.GetFeaturedAsync(6);
        var recentGuides = await _guideService.GetRecentAsync(12);

        var viewModel = new HomeViewModel
        {
            Categories = categories,
            FeaturedGuides = featuredGuides,
            RecentGuides = recentGuides,
            Language = language
        };

        // Log visit
        await LogVisitAsync(language == "en" ? "/en/guides" : "/rehberler");

        return View("Index", viewModel);
    }

    // GET: /rehberler/{categorySlug}
    [HttpGet("rehberler/{categorySlug}")]
    public async Task<IActionResult> Category(string categorySlug)
    {
        return await GetCategoryView(categorySlug, "tr");
    }

    // GET: /en/guides/{categorySlug}
    [HttpGet("en/guides/{categorySlug}")]
    public async Task<IActionResult> CategoryEn(string categorySlug)
    {
        return await GetCategoryView(categorySlug, "en");
    }

    private async Task<IActionResult> GetCategoryView(string categorySlug, string language)
    {
        var category = await _categoryService.GetBySlugAsync(categorySlug, language);
        if (category == null)
        {
            // Try the other language
            category = await _categoryService.GetBySlugAsync(categorySlug, language == "en" ? "tr" : "en");
        }

        if (category == null)
        {
            return NotFound();
        }

        var guides = await _guideService.GetByCategoryAsync(category.Id, true);
        var allCategories = await _categoryService.GetAllAsync(true);

        var viewModel = new CategoryDetailViewModel
        {
            Category = category,
            Guides = guides,
            AllCategories = allCategories,
            Language = language
        };

        // Log visit
        var slug = language == "en" && !string.IsNullOrEmpty(category.SlugEn) ? category.SlugEn : category.SlugTr;
        await LogVisitAsync(language == "en" ? $"/en/guides/{slug}" : $"/rehberler/{slug}");

        return View("Category", viewModel);
    }

    // GET: /rehberler/{categorySlug}/{guideSlug}
    [HttpGet("rehberler/{categorySlug}/{guideSlug}")]
    public async Task<IActionResult> Detail(string categorySlug, string guideSlug)
    {
        return await GetDetailView(categorySlug, guideSlug, "tr");
    }

    // GET: /en/guides/{categorySlug}/{guideSlug}
    [HttpGet("en/guides/{categorySlug}/{guideSlug}")]
    public async Task<IActionResult> DetailEn(string categorySlug, string guideSlug)
    {
        return await GetDetailView(categorySlug, guideSlug, "en");
    }

    private async Task<IActionResult> GetDetailView(string categorySlug, string guideSlug, string language)
    {
        // First verify the category exists
        var category = await _categoryService.GetBySlugAsync(categorySlug, language);
        if (category == null)
        {
            category = await _categoryService.GetBySlugAsync(categorySlug, language == "en" ? "tr" : "en");
        }

        if (category == null)
        {
            return NotFound();
        }

        // Get the guide
        var guide = await _guideService.GetBySlugAsync(guideSlug, language);
        if (guide == null)
        {
            guide = await _guideService.GetBySlugAsync(guideSlug, language == "en" ? "tr" : "en");
        }

        if (guide == null || guide.CategoryId != category.Id)
        {
            return NotFound();
        }

        // Increment view count
        await _guideService.IncrementViewCountAsync(guide.Id);

        // Get related guides
        var relatedGuides = await _guideService.GetRelatedGuidesAsync(guide.Id);

        // Get all categories for sidebar
        var allCategories = await _categoryService.GetAllAsync(true);

        // Get code blocks
        var codeBlocks = await _guideService.GetCodeBlocksByGuideIdAsync(guide.Id);

        var viewModel = new GuideDetailViewModel
        {
            Guide = guide,
            Category = category,
            RelatedGuides = relatedGuides,
            AllCategories = allCategories,
            CodeBlocks = codeBlocks,
            Language = language
        };

        // Log visit
        var catSlug = language == "en" && !string.IsNullOrEmpty(category.SlugEn) ? category.SlugEn : category.SlugTr;
        var gSlug = language == "en" && !string.IsNullOrEmpty(guide.SlugEn) ? guide.SlugEn : guide.SlugTr;
        await LogVisitAsync(language == "en" ? $"/en/guides/{catSlug}/{gSlug}" : $"/rehberler/{catSlug}/{gSlug}");

        return View("Detail", viewModel);
    }

    private async Task LogVisitAsync(string path)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
            var referer = HttpContext.Request.Headers.Referer.ToString();

            await _visitorLogService.LogVisitAsync(path, ipAddress, userAgent, referer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log visit for {Path}", path);
        }
    }
}
