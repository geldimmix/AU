using AlgoritmaUzmani.Data;
using AlgoritmaUzmani.Services;
using AlgoritmaUzmani.Services.Interfaces;
using AlgoritmaUzmani.Modules.Guides;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddApplicationPart(typeof(AlgoritmaUzmani.Modules.Guides.Controllers.GuidesController).Assembly)
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Distributed Cache (Memory Cache)
builder.Services.AddDistributedMemoryCache();

// Memory Cache for CacheService
builder.Services.AddMemoryCache();

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/admin/login";
        options.LogoutPath = "/admin/logout";
        options.AccessDeniedPath = "/admin/login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.Name = "AlgoritmaUzmani.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// Register Core Services
builder.Services.AddSingleton<ICacheService, CacheService>(); // Singleton olmalı - _keys HashSet tüm uygulama boyunca korunmalı
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IGuideService, GuideService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<ISeoTagService, SeoTagService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IStaticPageService, StaticPageService>();
builder.Services.AddScoped<IVisitorLogService, VisitorLogService>();
builder.Services.AddScoped<ISiteSettingService, SiteSettingService>();

// HttpClient for external API calls (with extended timeout for AI translation)
builder.Services.AddHttpClient<ITranslationService, TranslationService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5); // 5 dakika timeout - uzun içerikler için
});
builder.Services.AddHttpClient<ICodeTranslationService, CodeTranslationService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5); // 5 dakika timeout - çoklu dil çevirisi için
});
builder.Services.AddHttpClient(); // Genel HTTP istekleri için

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// AppData klasörünü /appdata yolundan serve et
var appDataPath = Path.Combine(app.Environment.ContentRootPath, "AppData");
if (!Directory.Exists(appDataPath))
    Directory.CreateDirectory(appDataPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(appDataPath),
    RequestPath = "/appdata"
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Map module routes
app.MapGuidesModule();

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
