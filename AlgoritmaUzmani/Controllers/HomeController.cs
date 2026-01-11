using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AlgoritmaUzmani.Models;
using AlgoritmaUzmani.Models.ViewModels.Public;
using AlgoritmaUzmani.Services.Interfaces;

namespace AlgoritmaUzmani.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ICategoryService _categoryService;
    private readonly IGuideService _guideService;

    public HomeController(
        ILogger<HomeController> logger,
        ICategoryService categoryService,
        IGuideService guideService)
    {
        _logger = logger;
        _categoryService = categoryService;
        _guideService = guideService;
    }

    [HttpGet("/")]
    [HttpGet("/en")]
    public async Task<IActionResult> Index()
    {
        var language = Request.Path.StartsWithSegments("/en") ? "en" : "tr";
        var categories = await _categoryService.GetAllAsync();
        var featuredGuides = await _guideService.GetFeaturedAsync(3);
        var recentGuides = await _guideService.GetRecentAsync(6);

        var model = new HomeViewModel
        {
            Categories = categories,
            FeaturedGuides = featuredGuides,
            RecentGuides = recentGuides,
            Language = language
        };

        ViewBag.Title = language == "en" ? "Algorithm Expert - Programming Guides" : "Algoritma Uzmanı - Programlama Rehberleri";
        ViewBag.MetaDescription = language == "en"
            ? "Learn data structures, algorithms, software architecture and more with comprehensive guides."
            : "Veri yapıları, algoritmalar, yazılım mimarisi ve daha fazlasını kapsamlı rehberlerle öğrenin.";
        ViewBag.Language = language;
        ViewBag.CanonicalUrl = language == "en" ? "/en" : "/";
        ViewBag.AlternateUrl = language == "en" ? "/" : "/en";

        return View(model);
    }

    [HttpGet("/sitemap.xml")]
    public async Task<IActionResult> Sitemap()
    {
        var categories = await _categoryService.GetAllAsync();
        var guides = await _guideService.GetAllAsync();

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var sitemap = new System.Text.StringBuilder();
        
        sitemap.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sitemap.AppendLine("<?xml-stylesheet type=\"text/xsl\" href=\"/sitemap.xsl\"?>");
        sitemap.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\" xmlns:xhtml=\"http://www.w3.org/1999/xhtml\">");

        // Home page
        sitemap.AppendLine("  <url>");
        sitemap.AppendLine($"    <loc>{baseUrl}/</loc>");
        sitemap.AppendLine($"    <xhtml:link rel=\"alternate\" hreflang=\"tr\" href=\"{baseUrl}/\" />");
        sitemap.AppendLine($"    <xhtml:link rel=\"alternate\" hreflang=\"en\" href=\"{baseUrl}/en\" />");
        sitemap.AppendLine("    <changefreq>daily</changefreq>");
        sitemap.AppendLine("    <priority>1.0</priority>");
        sitemap.AppendLine("  </url>");

        // Guides page
        sitemap.AppendLine("  <url>");
        sitemap.AppendLine($"    <loc>{baseUrl}/rehberler</loc>");
        sitemap.AppendLine($"    <xhtml:link rel=\"alternate\" hreflang=\"tr\" href=\"{baseUrl}/rehberler\" />");
        sitemap.AppendLine($"    <xhtml:link rel=\"alternate\" hreflang=\"en\" href=\"{baseUrl}/en/guides\" />");
        sitemap.AppendLine("    <changefreq>daily</changefreq>");
        sitemap.AppendLine("    <priority>0.9</priority>");
        sitemap.AppendLine("  </url>");

        // Categories
        foreach (var category in categories)
        {
            sitemap.AppendLine("  <url>");
            sitemap.AppendLine($"    <loc>{baseUrl}/rehberler/{category.SlugTr}</loc>");
            sitemap.AppendLine($"    <xhtml:link rel=\"alternate\" hreflang=\"tr\" href=\"{baseUrl}/rehberler/{category.SlugTr}\" />");
            sitemap.AppendLine($"    <xhtml:link rel=\"alternate\" hreflang=\"en\" href=\"{baseUrl}/en/guides/{category.SlugEn ?? category.SlugTr}\" />");
            sitemap.AppendLine("    <changefreq>weekly</changefreq>");
            sitemap.AppendLine("    <priority>0.8</priority>");
            sitemap.AppendLine("  </url>");
        }

        // Guides
        foreach (var guide in guides)
        {
            var categorySlugTr = guide.Category.SlugTr;
            var categorySlugEn = guide.Category.SlugEn ?? guide.Category.SlugTr;
            var guideSlugEn = guide.SlugEn ?? guide.SlugTr;

            sitemap.AppendLine("  <url>");
            sitemap.AppendLine($"    <loc>{baseUrl}/rehberler/{categorySlugTr}/{guide.SlugTr}</loc>");
            sitemap.AppendLine($"    <xhtml:link rel=\"alternate\" hreflang=\"tr\" href=\"{baseUrl}/rehberler/{categorySlugTr}/{guide.SlugTr}\" />");
            sitemap.AppendLine($"    <xhtml:link rel=\"alternate\" hreflang=\"en\" href=\"{baseUrl}/en/guides/{categorySlugEn}/{guideSlugEn}\" />");
            sitemap.AppendLine($"    <lastmod>{guide.UpdatedAt?.ToString("yyyy-MM-dd") ?? guide.CreatedAt.ToString("yyyy-MM-dd")}</lastmod>");
            sitemap.AppendLine("    <changefreq>weekly</changefreq>");
            sitemap.AppendLine("    <priority>0.9</priority>");
            sitemap.AppendLine("  </url>");
        }

        // Static pages
        var staticPages = new[] { "hakkinda", "gizlilik", "cerez-politikasi" };
        foreach (var slug in staticPages)
        {
            sitemap.AppendLine("  <url>");
            sitemap.AppendLine($"    <loc>{baseUrl}/sayfa/{slug}</loc>");
            sitemap.AppendLine($"    <xhtml:link rel=\"alternate\" hreflang=\"tr\" href=\"{baseUrl}/sayfa/{slug}\" />");
            sitemap.AppendLine($"    <xhtml:link rel=\"alternate\" hreflang=\"en\" href=\"{baseUrl}/en/page/{slug}\" />");
            sitemap.AppendLine("    <changefreq>monthly</changefreq>");
            sitemap.AppendLine("    <priority>0.5</priority>");
            sitemap.AppendLine("  </url>");
        }

        sitemap.AppendLine("</urlset>");

        return Content(sitemap.ToString(), "application/xml");
    }

    [HttpGet("/robots.txt")]
    public IActionResult Robots()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var robots = $@"User-agent: *
Allow: /

Sitemap: {baseUrl}/sitemap.xml";

        return Content(robots, "text/plain");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [Route("/404")]
    [Route("/en/404")]
    public IActionResult PageNotFound()
    {
        Response.StatusCode = 404;
        return View("NotFound");
    }
}
