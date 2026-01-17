using AlgoritmaUzmani.Models.ViewModels.Public;
using AlgoritmaUzmani.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AlgoritmaUzmani.Controllers;

public class GuidesController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly IGuideService _guideService;
    private readonly IConfiguration _configuration;

    public GuidesController(
        ICategoryService categoryService,
        IGuideService guideService,
        IConfiguration configuration)
    {
        _categoryService = categoryService;
        _guideService = guideService;
        _configuration = configuration;
    }

    // GET: /rehberler or /en/guides
    [HttpGet("/rehberler")]
    [HttpGet("/en/guides")]
    public async Task<IActionResult> Index()
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

    // GET: /rehberler/{categorySlug} or /en/guides/{categorySlug}
    [HttpGet("/rehberler/{categorySlug}")]
    [HttpGet("/en/guides/{categorySlug}")]
    public async Task<IActionResult> Category(string categorySlug)
    {
        var language = Request.Path.StartsWithSegments("/en") ? "en" : "tr";
        var category = await _categoryService.GetBySlugAsync(categorySlug, language);

        if (category == null)
            return NotFound();

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

        return View(model);
    }

    // GET: /rehberler/{categorySlug}/{guideSlug} or /en/guides/{categorySlug}/{guideSlug}
    [HttpGet("/rehberler/{categorySlug}/{guideSlug}")]
    [HttpGet("/en/guides/{categorySlug}/{guideSlug}")]
    public async Task<IActionResult> Detail(string categorySlug, string guideSlug)
    {
        var language = Request.Path.StartsWithSegments("/en") ? "en" : "tr";
        var guide = await _guideService.GetBySlugAsync(guideSlug, language);

        if (guide == null)
            return NotFound();

        // Verify category slug matches
        var expectedCategorySlug = language == "en" && !string.IsNullOrEmpty(guide.Category.SlugEn) 
            ? guide.Category.SlugEn 
            : guide.Category.SlugTr;

        if (!string.Equals(categorySlug, expectedCategorySlug, StringComparison.OrdinalIgnoreCase))
        {
            // Redirect to correct URL
            var correctUrl = language == "en" 
                ? $"/en/guides/{expectedCategorySlug}/{guideSlug}" 
                : $"/rehberler/{expectedCategorySlug}/{guideSlug}";
            return RedirectPermanent(correctUrl);
        }

        // Increment view count
        await _guideService.IncrementViewCountAsync(guide.Id);

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
        var seoTags = guide.GuideSeoTags.Select(st => 
            language == "en" && !string.IsNullOrEmpty(st.SeoTag.NameEn) 
                ? st.SeoTag.NameEn 
                : st.SeoTag.NameTr
        ).ToList();
        ViewBag.SeoTags = seoTags;

        return View(model);
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

