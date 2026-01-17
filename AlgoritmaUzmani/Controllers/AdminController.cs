using AlgoritmaUzmani.Models.Entities;
using AlgoritmaUzmani.Models.ViewModels.Admin;
using AlgoritmaUzmani.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace AlgoritmaUzmani.Controllers;

[Route("admin")]
public class AdminController : Controller
{
    private readonly IAdminAuthService _authService;
    private readonly ICategoryService _categoryService;
    private readonly IGuideService _guideService;
    private readonly ITagService _tagService;
    private readonly ISeoTagService _seoTagService;
    private readonly IImageService _imageService;
    private readonly ITranslationService _translationService;
    private readonly ICodeTranslationService _codeTranslationService;
    private readonly ICacheService _cacheService;
    private readonly IStaticPageService _staticPageService;
    private readonly IVisitorLogService _visitorLogService;
    private readonly ISiteSettingService _siteSettingService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IAdminAuthService authService,
        ICategoryService categoryService,
        IGuideService guideService,
        ITagService tagService,
        ISeoTagService seoTagService,
        IImageService imageService,
        ITranslationService translationService,
        ICodeTranslationService codeTranslationService,
        ICacheService cacheService,
        IStaticPageService staticPageService,
        IVisitorLogService visitorLogService,
        ISiteSettingService siteSettingService,
        ILogger<AdminController> logger)
    {
        _authService = authService;
        _categoryService = categoryService;
        _guideService = guideService;
        _tagService = tagService;
        _seoTagService = seoTagService;
        _imageService = imageService;
        _translationService = translationService;
        _codeTranslationService = codeTranslationService;
        _cacheService = cacheService;
        _staticPageService = staticPageService;
        _visitorLogService = visitorLogService;
        _siteSettingService = siteSettingService;
        _logger = logger;
    }

    #region Authentication

    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction(nameof(Dashboard));

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _authService.ValidateCredentialsAsync(model.Username, model.Password);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(model.RememberMe ? 30 : 1)
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
        await _authService.UpdateLastLoginAsync(user.Id);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost("logout")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    #endregion

    #region Dashboard

    [HttpGet("")]
    [HttpGet("dashboard")]
    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        var categories = await _categoryService.GetAllAsync(false);
        var guides = await _guideService.GetAllAsync(false);

        ViewBag.TotalCategories = categories.Count;
        ViewBag.TotalGuides = guides.Count;
        ViewBag.ActiveGuides = guides.Count(g => g.IsActive);
        ViewBag.TranslatedGuides = guides.Count(g => g.IsTranslated);
        ViewBag.RecentGuides = guides.OrderByDescending(g => g.CreatedAt).Take(5).ToList();

        return View();
    }

    #endregion

    #region Categories

    [HttpGet("categories")]
    [Authorize]
    public async Task<IActionResult> Categories()
    {
        var categories = await _categoryService.GetAllAsync(false);
        return View(categories);
    }

    [HttpGet("categories/create")]
    [Authorize]
    public IActionResult CreateCategory()
    {
        return View(new CategoryViewModel());
    }

    [HttpPost("categories/create")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(CategoryViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var category = new Category
        {
            NameTr = model.NameTr,
            NameEn = model.NameEn,
            DescriptionTr = model.DescriptionTr,
            DescriptionEn = model.DescriptionEn,
            Icon = model.Icon,
            DisplayOrder = model.DisplayOrder,
            IsActive = model.IsActive
        };

        await _categoryService.CreateAsync(category);
        TempData["Success"] = "Kategori başarıyla oluşturuldu";
        return RedirectToAction(nameof(Categories));
    }

    [HttpGet("categories/edit/{id}")]
    [Authorize]
    public async Task<IActionResult> EditCategory(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null)
            return NotFound();

        var model = new CategoryViewModel
        {
            Id = category.Id,
            NameTr = category.NameTr,
            NameEn = category.NameEn,
            DescriptionTr = category.DescriptionTr,
            DescriptionEn = category.DescriptionEn,
            Icon = category.Icon,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            GuideCount = category.Guides.Count
        };

        return View(model);
    }

    [HttpPost("categories/edit/{id}")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(int id, CategoryViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var category = new Category
        {
            Id = model.Id,
            NameTr = model.NameTr,
            NameEn = model.NameEn,
            DescriptionTr = model.DescriptionTr,
            DescriptionEn = model.DescriptionEn,
            Icon = model.Icon,
            DisplayOrder = model.DisplayOrder,
            IsActive = model.IsActive
        };

        await _categoryService.UpdateAsync(category);
        TempData["Success"] = "Kategori başarıyla güncellendi";
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost("categories/delete/{id}")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            await _categoryService.DeleteAsync(id);
            TempData["Success"] = "Kategori başarıyla silindi";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Categories));
    }

    #endregion

    #region Guides

    [HttpGet("guides")]
    [Authorize]
    public async Task<IActionResult> Guides()
    {
        var guides = await _guideService.GetAllAsync(false);
        return View(guides);
    }

    [HttpGet("guides/create")]
    [Authorize]
    public async Task<IActionResult> CreateGuide()
    {
        var model = new GuideViewModel();
        await PopulateGuideSelectLists(model);
        return View(model);
    }

    [HttpPost("guides/create")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateGuide(GuideViewModel model, IFormFile? featuredImage)
    {
        if (!ModelState.IsValid)
        {
            await PopulateGuideSelectLists(model);
            return View(model);
        }

        // Handle image upload
        if (featuredImage != null)
        {
            try
            {
                model.FeaturedImage = await _imageService.SaveImageAsync(featuredImage, "guides");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("FeaturedImage", ex.Message);
                await PopulateGuideSelectLists(model);
                return View(model);
            }
        }

        var guide = new Guide
        {
            CategoryId = model.CategoryId,
            TitleTr = model.TitleTr,
            SummaryTr = model.SummaryTr,
            ContentTr = model.ContentTr,
            MetaDescriptionTr = model.MetaDescriptionTr,
            SeoKeywordsTr = model.SeoKeywordsTr,
            FeaturedImage = model.FeaturedImage,
            FeaturedImageAltTr = model.FeaturedImageAltTr,
            IsFeatured = model.IsFeatured,
            DisplayOrder = model.DisplayOrder,
            IsActive = model.IsActive,
            PublishedAt = model.IsActive ? DateTime.UtcNow : null
        };

        // Auto-translate to English
        try
        {
            var (titleEn, contentEn, summaryEn, metaDescriptionEn) = await _translationService.TranslateGuideAsync(
                model.TitleTr, model.ContentTr, model.SummaryTr, model.MetaDescriptionTr);
            
            guide.TitleEn = titleEn;
            guide.ContentEn = contentEn;
            guide.SummaryEn = summaryEn;
            guide.MetaDescriptionEn = metaDescriptionEn;
            guide.FeaturedImageAltEn = !string.IsNullOrEmpty(model.FeaturedImageAltTr) 
                ? await _translationService.TranslateToEnglishAsync(model.FeaturedImageAltTr) 
                : null;
            guide.IsTranslated = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to auto-translate guide: {Title}", model.TitleTr);
            guide.IsTranslated = false;
        }

        var created = await _guideService.CreateAsync(guide);

        // Set tags
        if (model.SelectedTagIds.Any())
            await _guideService.SetTagsAsync(created.Id, model.SelectedTagIds);

        if (model.SelectedSeoTagIds.Any())
            await _guideService.SetSeoTagsAsync(created.Id, model.SelectedSeoTagIds);

        if (model.RelatedGuideIds.Any())
            await _guideService.SetRelatedGuidesAsync(created.Id, model.RelatedGuideIds);

        TempData["Success"] = "Rehber başarıyla oluşturuldu";
        return RedirectToAction(nameof(EditGuide), new { id = created.Id });
    }

    [HttpGet("guides/edit/{id}")]
    [Authorize]
    public async Task<IActionResult> EditGuide(int id)
    {
        var guide = await _guideService.GetByIdWithRelationsAsync(id);
        if (guide == null)
            return NotFound();

        var model = new GuideViewModel
        {
            Id = guide.Id,
            CategoryId = guide.CategoryId,
            TitleTr = guide.TitleTr,
            TitleEn = guide.TitleEn,
            SummaryTr = guide.SummaryTr,
            SummaryEn = guide.SummaryEn,
            ContentTr = guide.ContentTr,
            ContentEn = guide.ContentEn,
            MetaDescriptionTr = guide.MetaDescriptionTr,
            MetaDescriptionEn = guide.MetaDescriptionEn,
            SeoKeywordsTr = guide.SeoKeywordsTr,
            SeoKeywordsEn = guide.SeoKeywordsEn,
            FeaturedImage = guide.FeaturedImage,
            FeaturedImageAltTr = guide.FeaturedImageAltTr,
            FeaturedImageAltEn = guide.FeaturedImageAltEn,
            IsFeatured = guide.IsFeatured,
            DisplayOrder = guide.DisplayOrder,
            IsActive = guide.IsActive,
            IsTranslated = guide.IsTranslated,
            SelectedTagIds = guide.GuideTags.Select(gt => gt.TagId).ToList(),
            SelectedSeoTagIds = guide.GuideSeoTags.Select(gs => gs.SeoTagId).ToList(),
            RelatedGuideIds = guide.RelatedGuides.Select(rg => rg.RelatedGuideId).ToList(),
            CreatedAt = guide.CreatedAt,
            UpdatedAt = guide.UpdatedAt,
            ViewCount = guide.ViewCount
        };

        await PopulateGuideSelectLists(model, id);
        return View(model);
    }

    [HttpPost("guides/edit/{id}")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditGuide(int id, GuideViewModel model, IFormFile? featuredImage)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateGuideSelectLists(model, id);
            return View(model);
        }

        // Handle image upload
        if (featuredImage != null)
        {
            try
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(model.FeaturedImage))
                    await _imageService.DeleteImageAsync(model.FeaturedImage);

                model.FeaturedImage = await _imageService.SaveImageAsync(featuredImage, "guides");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("FeaturedImage", ex.Message);
                await PopulateGuideSelectLists(model, id);
                return View(model);
            }
        }

        var guide = new Guide
        {
            Id = model.Id,
            CategoryId = model.CategoryId,
            TitleTr = model.TitleTr,
            TitleEn = model.TitleEn,
            SummaryTr = model.SummaryTr,
            SummaryEn = model.SummaryEn,
            ContentTr = model.ContentTr,
            ContentEn = model.ContentEn,
            MetaDescriptionTr = model.MetaDescriptionTr,
            MetaDescriptionEn = model.MetaDescriptionEn,
            SeoKeywordsTr = model.SeoKeywordsTr,
            SeoKeywordsEn = model.SeoKeywordsEn,
            FeaturedImage = model.FeaturedImage,
            FeaturedImageAltTr = model.FeaturedImageAltTr,
            FeaturedImageAltEn = model.FeaturedImageAltEn,
            IsFeatured = model.IsFeatured,
            DisplayOrder = model.DisplayOrder,
            IsActive = model.IsActive,
            IsTranslated = model.IsTranslated
        };

        await _guideService.UpdateAsync(guide);

        // Update relations
        await _guideService.SetTagsAsync(id, model.SelectedTagIds);
        await _guideService.SetSeoTagsAsync(id, model.SelectedSeoTagIds);
        await _guideService.SetRelatedGuidesAsync(id, model.RelatedGuideIds);

        TempData["Success"] = "Rehber başarıyla güncellendi";
        return RedirectToAction(nameof(EditGuide), new { id });
    }

    [HttpPost("guides/delete/{id}")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteGuide(int id)
    {
        var guide = await _guideService.GetByIdAsync(id);
        if (guide?.FeaturedImage != null)
            await _imageService.DeleteImageAsync(guide.FeaturedImage);

        await _guideService.DeleteAsync(id);
        TempData["Success"] = "Rehber başarıyla silindi";
        return RedirectToAction(nameof(Guides));
    }

    [HttpPost("guides/translate/{id}")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TranslateGuide(int id)
    {
        try
        {
            var guide = await _guideService.GetByIdAsync(id);
            if (guide == null)
                return NotFound();

            var (titleEn, contentEn, summaryEn, metaDescriptionEn) = await _translationService.TranslateGuideAsync(
                guide.TitleTr,
                guide.ContentTr,
                guide.SummaryTr,
                guide.MetaDescriptionTr
            );

            guide.TitleEn = titleEn;
            guide.ContentEn = contentEn;
            guide.SummaryEn = summaryEn;
            guide.MetaDescriptionEn = metaDescriptionEn;
            guide.IsTranslated = true;

            await _guideService.UpdateAsync(guide);

            TempData["Success"] = "Rehber başarıyla çevrildi";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Translation failed for guide {GuideId}", id);
            TempData["Error"] = "Çeviri sırasında bir hata oluştu";
        }

        return RedirectToAction(nameof(EditGuide), new { id });
    }

    #endregion

    #region Tags

    [HttpGet("tags")]
    [Authorize]
    public async Task<IActionResult> Tags()
    {
        var tags = await _tagService.GetAllAsync();
        return View(tags);
    }

    [HttpGet("tags/create")]
    [Authorize]
    public IActionResult CreateTag()
    {
        return View(new TagViewModel());
    }

    [HttpPost("tags/create")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTag(TagViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var tag = new Tag
        {
            NameTr = model.NameTr,
            NameEn = model.NameEn,
            Color = model.Color
        };

        await _tagService.CreateAsync(tag);
        TempData["Success"] = "Etiket başarıyla oluşturuldu";
        return RedirectToAction(nameof(Tags));
    }

    [HttpGet("tags/edit/{id}")]
    [Authorize]
    public async Task<IActionResult> EditTag(int id)
    {
        var tag = await _tagService.GetByIdAsync(id);
        if (tag == null)
            return NotFound();

        var model = new TagViewModel
        {
            Id = tag.Id,
            NameTr = tag.NameTr,
            NameEn = tag.NameEn,
            Color = tag.Color
        };

        return View(model);
    }

    [HttpPost("tags/edit/{id}")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTag(int id, TagViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var tag = new Tag
        {
            Id = model.Id,
            NameTr = model.NameTr,
            NameEn = model.NameEn,
            Color = model.Color
        };

        await _tagService.UpdateAsync(tag);
        TempData["Success"] = "Etiket başarıyla güncellendi";
        return RedirectToAction(nameof(Tags));
    }

    [HttpPost("tags/delete/{id}")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTag(int id)
    {
        await _tagService.DeleteAsync(id);
        TempData["Success"] = "Etiket başarıyla silindi";
        return RedirectToAction(nameof(Tags));
    }

    #endregion

    #region SEO Tags

    [HttpGet("seotags")]
    [Authorize]
    public async Task<IActionResult> SeoTags()
    {
        var seoTags = await _seoTagService.GetAllAsync();
        return View(seoTags);
    }

    [HttpGet("seotags/create")]
    [Authorize]
    public IActionResult CreateSeoTag()
    {
        return View(new SeoTagViewModel());
    }

    [HttpPost("seotags/create")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSeoTag(SeoTagViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var seoTag = new SeoTag
        {
            NameTr = model.NameTr,
            NameEn = model.NameEn
        };

        await _seoTagService.CreateAsync(seoTag);
        TempData["Success"] = "SEO etiketi başarıyla oluşturuldu";
        return RedirectToAction(nameof(SeoTags));
    }

    [HttpGet("seotags/edit/{id}")]
    [Authorize]
    public async Task<IActionResult> EditSeoTag(int id)
    {
        var seoTag = await _seoTagService.GetByIdAsync(id);
        if (seoTag == null)
            return NotFound();

        var model = new SeoTagViewModel
        {
            Id = seoTag.Id,
            NameTr = seoTag.NameTr,
            NameEn = seoTag.NameEn
        };

        return View(model);
    }

    [HttpPost("seotags/edit/{id}")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSeoTag(int id, SeoTagViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var seoTag = new SeoTag
        {
            Id = model.Id,
            NameTr = model.NameTr,
            NameEn = model.NameEn
        };

        await _seoTagService.UpdateAsync(seoTag);
        TempData["Success"] = "SEO etiketi başarıyla güncellendi";
        return RedirectToAction(nameof(SeoTags));
    }

    [HttpPost("seotags/delete/{id}")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSeoTag(int id)
    {
        await _seoTagService.DeleteAsync(id);
        TempData["Success"] = "SEO etiketi başarıyla silindi";
        return RedirectToAction(nameof(SeoTags));
    }

    #endregion

    #region Image Upload

    [HttpPost("upload-image")]
    [Authorize]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        try
        {
            var path = await _imageService.SaveImageAsync(file, "content");
            var url = _imageService.GetImageUrl(path);
            return Json(new { success = true, url });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    #endregion

    #region Code Translation

    [HttpPost("translate-code")]
    [Authorize]
    public async Task<IActionResult> TranslateCode([FromBody] CodeTranslationRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.SourceCode))
            {
                return Json(new { success = false, error = "Kaynak kod gerekli" });
            }

            if (string.IsNullOrWhiteSpace(request.SourceLanguage))
            {
                return Json(new { success = false, error = "Kaynak dil gerekli" });
            }

            if (request.TargetLanguages == null || !request.TargetLanguages.Any())
            {
                return Json(new { success = false, error = "En az bir hedef dil seçilmeli" });
            }

            var translations = await _codeTranslationService.TranslateCodeToMultipleLanguagesAsync(
                request.SourceCode,
                request.SourceLanguage,
                request.TargetLanguages
            );

            return Json(new { success = true, translations });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Code translation error");
            return Json(new { success = false, error = ex.Message });
        }
    }

    #endregion

    #region Cache Management

    [HttpPost("clear-cache")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearCache()
    {
        await _cacheService.ClearAllAsync();
        TempData["Success"] = "Önbellek temizlendi";
        return RedirectToAction(nameof(Dashboard));
    }

    #endregion

    #region SEO Suggestions

    [HttpPost("guides/seo-suggestions")]
    [Authorize]
    public async Task<IActionResult> GetSeoSuggestions([FromBody] SeoSuggestionRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Title) && string.IsNullOrWhiteSpace(request.Content))
            {
                return Json(new { success = false, error = "Başlık veya içerik gerekli" });
            }

            var (metaDescription, keywords) = await _translationService.GenerateSeoSuggestionsAsync(
                request.Title ?? string.Empty, 
                request.Content ?? string.Empty);

            return Json(new { 
                success = true, 
                metaDescription, 
                keywords = string.Join(", ", keywords)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SEO suggestion generation failed");
            return Json(new { success = false, error = "SEO önerileri oluşturulurken bir hata oluştu" });
        }
    }

    public class SeoSuggestionRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
    }

    public class CodeTranslationRequest
    {
        public string SourceCode { get; set; } = string.Empty;
        public string SourceLanguage { get; set; } = string.Empty;
        public List<string> TargetLanguages { get; set; } = new();
    }

    #endregion

    #region Static Pages

    [HttpGet("pages")]
    [Authorize]
    public async Task<IActionResult> Pages()
    {
        var pages = await _staticPageService.GetAllAsync();
        return View(pages);
    }

    [HttpGet("pages/edit/{id}")]
    [Authorize]
    public async Task<IActionResult> EditPage(int id)
    {
        var page = await _staticPageService.GetByIdAsync(id);
        if (page == null)
            return NotFound();

        return View(page);
    }

    [HttpPost("pages/edit/{id}")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPage(int id, StaticPage model)
    {
        if (id != model.Id)
            return BadRequest();

        // Truncate meta descriptions
        if (!string.IsNullOrEmpty(model.MetaDescriptionTr) && model.MetaDescriptionTr.Length > 160)
            model.MetaDescriptionTr = model.MetaDescriptionTr.Substring(0, 157) + "...";
        if (!string.IsNullOrEmpty(model.MetaDescriptionEn) && model.MetaDescriptionEn.Length > 160)
            model.MetaDescriptionEn = model.MetaDescriptionEn.Substring(0, 157) + "...";

        await _staticPageService.UpdateAsync(model);
        TempData["Success"] = "Sayfa başarıyla güncellendi";
        return RedirectToAction(nameof(Pages));
    }

    #endregion

    #region Analytics

    [HttpGet("analytics")]
    [Authorize]
    public async Task<IActionResult> Analytics()
    {
        var today = DateTime.UtcNow.Date;
        var last30Days = today.AddDays(-29);

        ViewBag.TotalVisits = await _visitorLogService.GetTotalCountAsync(last30Days, today.AddDays(1));
        ViewBag.UniqueVisitors = await _visitorLogService.GetUniqueVisitorsCountAsync(last30Days, today.AddDays(1));
        ViewBag.TodayVisits = await _visitorLogService.GetTotalCountAsync(today, today.AddDays(1));
        ViewBag.BrowserStats = await _visitorLogService.GetBrowserStatsAsync(last30Days, today.AddDays(1));
        ViewBag.DeviceStats = await _visitorLogService.GetDeviceStatsAsync(last30Days, today.AddDays(1));
        ViewBag.OsStats = await _visitorLogService.GetOsStatsAsync(last30Days, today.AddDays(1));
        ViewBag.TopPages = await _visitorLogService.GetTopPagesAsync(10, last30Days, today.AddDays(1));
        ViewBag.DailyVisits = await _visitorLogService.GetDailyVisitsAsync(30);

        var recentVisitors = await _visitorLogService.GetRecentAsync(50);
        return View(recentVisitors);
    }

    [HttpGet("analytics/visitors")]
    [Authorize]
    public async Task<IActionResult> Visitors(int page = 1, string? startDate = null, string? endDate = null)
    {
        var start = string.IsNullOrEmpty(startDate) 
            ? DateTime.UtcNow.Date.AddDays(-30) 
            : DateTime.Parse(startDate);
        var end = string.IsNullOrEmpty(endDate) 
            ? DateTime.UtcNow.Date.AddDays(1) 
            : DateTime.Parse(endDate).AddDays(1);

        var visitors = await _visitorLogService.GetByDateRangeAsync(start, end, page, 100);
        var totalCount = await _visitorLogService.GetTotalCountAsync(start, end);

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / 100.0);
        ViewBag.TotalCount = totalCount;
        ViewBag.StartDate = start.ToString("yyyy-MM-dd");
        ViewBag.EndDate = end.AddDays(-1).ToString("yyyy-MM-dd");

        return View(visitors);
    }

    #endregion

    #region Site Settings

    [HttpGet("settings")]
    [Authorize]
    public async Task<IActionResult> Settings()
    {
        var headerScripts = await _siteSettingService.GetByKeyAsync("header_scripts");
        var footerScripts = await _siteSettingService.GetByKeyAsync("footer_scripts");
        var googleAnalytics = await _siteSettingService.GetByKeyAsync("google_analytics");
        var googleTagManager = await _siteSettingService.GetByKeyAsync("google_tag_manager");

        ViewBag.HeaderScripts = headerScripts?.Value ?? "";
        ViewBag.FooterScripts = footerScripts?.Value ?? "";
        ViewBag.GoogleAnalytics = googleAnalytics?.Value ?? "";
        ViewBag.GoogleTagManager = googleTagManager?.Value ?? "";

        return View();
    }

    [HttpPost("settings")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(string? headerScripts, string? footerScripts, string? googleAnalytics, string? googleTagManager)
    {
        await _siteSettingService.CreateOrUpdateAsync("header_scripts", headerScripts, 
            "Head bölümüne eklenecek scriptler (Google Analytics, vs.)", "Scripts");
        await _siteSettingService.CreateOrUpdateAsync("footer_scripts", footerScripts, 
            "Body sonuna eklenecek scriptler", "Scripts");
        await _siteSettingService.CreateOrUpdateAsync("google_analytics", googleAnalytics, 
            "Google Analytics Measurement ID (G-XXXXXXXXXX)", "Scripts");
        await _siteSettingService.CreateOrUpdateAsync("google_tag_manager", googleTagManager, 
            "Google Tag Manager Container ID (GTM-XXXXXXX)", "Scripts");

        TempData["Success"] = "Ayarlar başarıyla kaydedildi";
        return RedirectToAction(nameof(Settings));
    }

    #endregion

    #region Helpers

    private async Task PopulateGuideSelectLists(GuideViewModel model, int? excludeGuideId = null)
    {
        var categories = await _categoryService.GetAllAsync(false);
        var tags = await _tagService.GetAllAsync();
        var seoTags = await _seoTagService.GetAllAsync();
        var guides = await _guideService.GetAllAsync(false);

        model.Categories = new SelectList(categories, "Id", "NameTr", model.CategoryId);
        model.Tags = new SelectList(tags, "Id", "NameTr");
        model.SeoTags = new SelectList(seoTags, "Id", "NameTr");

        var otherGuides = guides.Where(g => g.Id != excludeGuideId).ToList();
        model.AllGuides = new SelectList(otherGuides, "Id", "TitleTr");
    }

    #endregion
}

