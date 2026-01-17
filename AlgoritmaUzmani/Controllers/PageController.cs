using AlgoritmaUzmani.Services.Interfaces;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoritmaUzmani.Controllers;

public class PageController : Controller
{
    private readonly IStaticPageService _pageService;
    private readonly ICategoryService _categoryService;
    private readonly IGuideService _guideService;

    public PageController(
        IStaticPageService pageService,
        ICategoryService categoryService,
        IGuideService guideService)
    {
        _pageService = pageService;
        _categoryService = categoryService;
        _guideService = guideService;
    }

    [Route("sayfa/{slug}")]
    [Route("en/page/{slug}")]
    public async Task<IActionResult> Show(string slug)
    {
        var page = await _pageService.GetBySlugAsync(slug);
        if (page == null)
            return NotFound();

        var requestCulture = HttpContext.Features.Get<IRequestCultureFeature>();
        var isEnglish = Request.Path.StartsWithSegments("/en");

        ViewBag.Language = isEnglish ? "en" : "tr";
        ViewBag.MetaDescription = isEnglish ? page.MetaDescriptionEn : page.MetaDescriptionTr;
        ViewBag.CanonicalUrl = isEnglish ? $"/en/page/{slug}" : $"/sayfa/{slug}";
        ViewBag.AlternateUrl = isEnglish ? $"/sayfa/{slug}" : $"/en/page/{slug}";
        ViewData["Title"] = (isEnglish ? page.TitleEn : page.TitleTr) + " - " + (isEnglish ? "Algorithm Expert" : "Algoritma Uzmanı");

        ViewBag.Categories = await _categoryService.GetAllAsync();
        ViewBag.IsEnglish = isEnglish;

        return View(page);
    }

    [Route("sitemap.xml")]
    public async Task<IActionResult> Sitemap()
    {
        var categories = await _categoryService.GetAllAsync();
        var guides = await _guideService.GetAllAsync();
        var pages = await _pageService.GetAllAsync();

        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9""
        xmlns:xhtml=""http://www.w3.org/1999/xhtml"">";

        // Home pages
        xml += $@"
  <url>
    <loc>{baseUrl}/</loc>
    <xhtml:link rel=""alternate"" hreflang=""tr"" href=""{baseUrl}/""/>
    <xhtml:link rel=""alternate"" hreflang=""en"" href=""{baseUrl}/en""/>
    <changefreq>daily</changefreq>
    <priority>1.0</priority>
  </url>
  <url>
    <loc>{baseUrl}/en</loc>
    <xhtml:link rel=""alternate"" hreflang=""en"" href=""{baseUrl}/en""/>
    <xhtml:link rel=""alternate"" hreflang=""tr"" href=""{baseUrl}/""/>
    <changefreq>daily</changefreq>
    <priority>1.0</priority>
  </url>";

        // Categories
        foreach (var cat in categories)
        {
            xml += $@"
  <url>
    <loc>{baseUrl}/rehberler/{cat.SlugTr}</loc>
    <xhtml:link rel=""alternate"" hreflang=""tr"" href=""{baseUrl}/rehberler/{cat.SlugTr}""/>
    <xhtml:link rel=""alternate"" hreflang=""en"" href=""{baseUrl}/en/guides/{cat.SlugEn}""/>
    <changefreq>weekly</changefreq>
    <priority>0.8</priority>
  </url>
  <url>
    <loc>{baseUrl}/en/guides/{cat.SlugEn}</loc>
    <xhtml:link rel=""alternate"" hreflang=""en"" href=""{baseUrl}/en/guides/{cat.SlugEn}""/>
    <xhtml:link rel=""alternate"" hreflang=""tr"" href=""{baseUrl}/rehberler/{cat.SlugTr}""/>
    <changefreq>weekly</changefreq>
    <priority>0.8</priority>
  </url>";
        }

        // Guides
        foreach (var guide in guides)
        {
            var catSlugTr = guide.Category?.SlugTr ?? "";
            var catSlugEn = guide.Category?.SlugEn ?? "";
            var lastMod = (guide.UpdatedAt ?? guide.CreatedAt).ToString("yyyy-MM-dd");

            xml += $@"
  <url>
    <loc>{baseUrl}/rehberler/{catSlugTr}/{guide.SlugTr}</loc>
    <lastmod>{lastMod}</lastmod>
    <xhtml:link rel=""alternate"" hreflang=""tr"" href=""{baseUrl}/rehberler/{catSlugTr}/{guide.SlugTr}""/>
    <xhtml:link rel=""alternate"" hreflang=""en"" href=""{baseUrl}/en/guides/{catSlugEn}/{guide.SlugEn}""/>
    <changefreq>monthly</changefreq>
    <priority>0.6</priority>
  </url>";

            if (!string.IsNullOrEmpty(guide.SlugEn))
            {
                xml += $@"
  <url>
    <loc>{baseUrl}/en/guides/{catSlugEn}/{guide.SlugEn}</loc>
    <lastmod>{lastMod}</lastmod>
    <xhtml:link rel=""alternate"" hreflang=""en"" href=""{baseUrl}/en/guides/{catSlugEn}/{guide.SlugEn}""/>
    <xhtml:link rel=""alternate"" hreflang=""tr"" href=""{baseUrl}/rehberler/{catSlugTr}/{guide.SlugTr}""/>
    <changefreq>monthly</changefreq>
    <priority>0.6</priority>
  </url>";
            }
        }

        // Static Pages
        foreach (var page in pages.Where(p => p.IsActive))
        {
            xml += $@"
  <url>
    <loc>{baseUrl}/sayfa/{page.Slug}</loc>
    <xhtml:link rel=""alternate"" hreflang=""tr"" href=""{baseUrl}/sayfa/{page.Slug}""/>
    <xhtml:link rel=""alternate"" hreflang=""en"" href=""{baseUrl}/en/page/{page.Slug}""/>
    <changefreq>monthly</changefreq>
    <priority>0.5</priority>
  </url>
  <url>
    <loc>{baseUrl}/en/page/{page.Slug}</loc>
    <xhtml:link rel=""alternate"" hreflang=""en"" href=""{baseUrl}/en/page/{page.Slug}""/>
    <xhtml:link rel=""alternate"" hreflang=""tr"" href=""{baseUrl}/sayfa/{page.Slug}""/>
    <changefreq>monthly</changefreq>
    <priority>0.5</priority>
  </url>";
        }

        xml += @"
</urlset>";

        return Content(xml, "application/xml");
    }
}



