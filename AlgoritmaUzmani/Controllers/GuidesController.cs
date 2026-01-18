using AlgoritmaUzmani.Models.ViewModels.Public;
using AlgoritmaUzmani.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AlgoritmaUzmani.Controllers;

public class GuidesController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly IGuideService _guideService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GuidesController> _logger;

    public GuidesController(
        ICategoryService categoryService,
        IGuideService guideService,
        IConfiguration configuration,
        ILogger<GuidesController> logger)
    {
        _categoryService = categoryService;
        _guideService = guideService;
        _configuration = configuration;
        _logger = logger;
    }

    // GET: /rehberler or /en/guides
    [HttpGet("/rehberler")]
    [HttpGet("/en/guides")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var language = Request.Path.StartsWithSegments("/en") ? "en" : "tr";
            var categories = await _categoryService.GetAllAsync();
            var featuredGuides = await _guideService.GetFeaturedAsync(3);
            var recentGuides = await _guideService.GetRecentAsync(10);

            var model = new HomeViewModel
            {
                Categories = categories,
                FeaturedGuides = featuredGuides,
                RecentGuides = recentGuides,
                Language = language
            };

            ViewBag.Title = language == "en" ? "Guides | Algorithm Expert" : "Rehberler | Algoritma Uzmanı";
            ViewBag.MetaDescription = language == "en" 
                ? "Comprehensive guides on data structures, algorithms, software architecture and more." 
                : "Veri yapıları, algoritmalar, yazılım mimarisi ve daha fazlası hakkında kapsamlı rehberler.";
            ViewBag.Language = language;

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading guides index page");
            return StatusCode(500, "An error occurred while loading the page. Please try again.");
        }
    }

    // GET: /rehberler/{categorySlug} or /en/guides/{categorySlug}
    [HttpGet("/rehberler/{categorySlug}")]
    [HttpGet("/en/guides/{categorySlug}")]
    public async Task<IActionResult> Category(string categorySlug)
    {
        try
        {
            _logger.LogInformation("Category page requested: {CategorySlug}", categorySlug);
            
            var language = Request.Path.StartsWithSegments("/en") ? "en" : "tr";
            var category = await _categoryService.GetBySlugAsync(categorySlug, language);

            if (category == null)
            {
                _logger.LogWarning("Category not found: {Slug} ({Language})", categorySlug, language);
                return NotFound();
            }

            var guides = await _guideService.GetByCategoryAsync(category.Id);
            var allCategories = await _categoryService.GetAllAsync();

            var model = new CategoryDetailViewModel
            {
                Category = category,
                Guides = guides,
                AllCategories = allCategories,
                Language = language
            };

            var categoryName = language == "en" && !string.IsNullOrEmpty(category.NameEn) 
                ? category.NameEn 
                : category.NameTr;
            var categoryDesc = language == "en" && !string.IsNullOrEmpty(category.DescriptionEn) 
                ? category.DescriptionEn 
                : category.DescriptionTr;

            ViewBag.Title = language == "en" 
                ? $"{categoryName} | Algorithm Expert" 
                : $"{categoryName} | Algoritma Uzmanı";
            ViewBag.MetaDescription = categoryDesc;
            ViewBag.Language = language;
            ViewBag.CanonicalUrl = language == "en" 
                ? $"/en/guides/{category.SlugEn ?? category.SlugTr}" 
                : $"/rehberler/{category.SlugTr}";
            ViewBag.AlternateUrl = language == "en" 
                ? $"/rehberler/{category.SlugTr}" 
                : $"/en/guides/{category.SlugEn ?? category.SlugTr}";

            _logger.LogInformation("Category page rendered: {CategoryId}", category.Id);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading category page: {CategorySlug}", categorySlug);
            return StatusCode(500, "An error occurred while loading the page. Please try again.");
        }
    }

    // GET: /rehberler/{categorySlug}/{guideSlug} or /en/guides/{categorySlug}/{guideSlug}
    [HttpGet("/rehberler/{categorySlug}/{guideSlug}")]
    [HttpGet("/en/guides/{categorySlug}/{guideSlug}")]
    public async Task<IActionResult> Detail(string categorySlug, string guideSlug)
    {
        try
        {
            _logger.LogInformation("Detail page requested: {CategorySlug}/{GuideSlug}", categorySlug, guideSlug);
            
            var language = Request.Path.StartsWithSegments("/en") ? "en" : "tr";
            var guide = await _guideService.GetBySlugAsync(guideSlug, language);

            if (guide == null)
            {
                _logger.LogWarning("Guide not found: {Slug} ({Language})", guideSlug, language);
                return NotFound();
            }

            // Null check for Category (critical!)
            if (guide.Category == null)
            {
                _logger.LogError("Guide found but Category is null: GuideId={GuideId}, Slug={Slug}", guide.Id, guideSlug);
                return NotFound();
            }

            // Verify category slug matches
            var expectedCategorySlug = language == "en" && !string.IsNullOrEmpty(guide.Category.SlugEn) 
                ? guide.Category.SlugEn 
                : guide.Category.SlugTr;

            if (!string.Equals(categorySlug, expectedCategorySlug, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Category slug mismatch, redirecting: {Expected} != {Actual}", expectedCategorySlug, categorySlug);
                // Redirect to correct URL
                var correctUrl = language == "en" 
                    ? $"/en/guides/{expectedCategorySlug}/{guideSlug}" 
                    : $"/rehberler/{expectedCategorySlug}/{guideSlug}";
                return RedirectPermanent(correctUrl);
            }

            // Increment view count (don't await to improve performance)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _guideService.IncrementViewCountAsync(guide.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to increment view count for guide {GuideId}", guide.Id);
                }
            });

            var relatedGuides = await _guideService.GetRelatedGuidesAsync(guide.Id);
            var allCategories = await _categoryService.GetAllAsync();
            var codeBlocks = await _guideService.GetCodeBlocksByGuideIdAsync(guide.Id);

            var model = new GuideDetailViewModel
            {
                Guide = guide,
                Category = guide.Category,
                RelatedGuides = relatedGuides,
                AllCategories = allCategories,
                CodeBlocks = codeBlocks,
                Language = language
            };

        ViewBag.Title = language == "en" 
            ? $"{model.Title} | Algorithm Expert" 
            : $"{model.Title} | Algoritma Uzmanı";
        ViewBag.MetaDescription = model.MetaDescription;
        ViewBag.MetaKeywords = model.Keywords;
        ViewBag.Language = language;
        
        var guideSlugTr = guide.SlugTr;
        var guideSlugEn = guide.SlugEn ?? guide.SlugTr;
        var categorySlugTr = guide.Category.SlugTr;
        var categorySlugEn = guide.Category.SlugEn ?? guide.Category.SlugTr;

        ViewBag.CanonicalUrl = language == "en" 
            ? $"/en/guides/{categorySlugEn}/{guideSlugEn}" 
            : $"/rehberler/{categorySlugTr}/{guideSlugTr}";
        ViewBag.AlternateUrl = language == "en" 
            ? $"/rehberler/{categorySlugTr}/{guideSlugTr}" 
            : $"/en/guides/{categorySlugEn}/{guideSlugEn}";

            // SEO Tags for schema.org
            var seoTags = guide.GuideSeoTags?.Select(st => 
                language == "en" && !string.IsNullOrEmpty(st.SeoTag?.NameEn) 
                    ? st.SeoTag.NameEn 
                    : st.SeoTag?.NameTr ?? ""
            ).ToList() ?? new List<string>();
            ViewBag.SeoTags = seoTags;

            _logger.LogInformation("Detail page rendered successfully: {GuideId}", guide.Id);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading guide detail: {CategorySlug}/{GuideSlug}", categorySlug, guideSlug);
            return StatusCode(500, "An error occurred while loading the page. Please try again.");
        }
    }

    // GET: /api/search?q=query&lang=tr
    [HttpGet("/api/search")]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] string lang = "tr")
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Json(new { results = Array.Empty<object>() });

        var guides = await _guideService.SearchAsync(q, lang, 8);

        var results = guides.Select(g => new
        {
            id = g.Id,
            title = lang == "en" && !string.IsNullOrEmpty(g.TitleEn) ? g.TitleEn : g.TitleTr,
            summary = lang == "en" && !string.IsNullOrEmpty(g.SummaryEn) 
                ? (g.SummaryEn.Length > 80 ? g.SummaryEn.Substring(0, 80) + "..." : g.SummaryEn)
                : (g.SummaryTr != null && g.SummaryTr.Length > 80 ? g.SummaryTr.Substring(0, 80) + "..." : g.SummaryTr),
            category = lang == "en" && !string.IsNullOrEmpty(g.Category.NameEn) ? g.Category.NameEn : g.Category.NameTr,
            categoryIcon = g.Category.Icon,
            url = lang == "en" 
                ? $"/en/guides/{g.Category.SlugEn ?? g.Category.SlugTr}/{g.SlugEn ?? g.SlugTr}"
                : $"/rehberler/{g.Category.SlugTr}/{g.SlugTr}"
        });

        return Json(new { results });
    }
}

