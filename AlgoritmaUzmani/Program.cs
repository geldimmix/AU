using AlgoritmaUzmani.Data;
using AlgoritmaUzmani.Middleware;
using AlgoritmaUzmani.Services;
using AlgoritmaUzmani.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Memory Cache
builder.Services.AddMemoryCache();

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/admin/login";
        options.LogoutPath = "/admin/logout";
        options.AccessDeniedPath = "/admin/login";
        options.Cookie.Name = "AlgoritmaUzmani.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

// HTTP Client for translation service
builder.Services.AddHttpClient<ITranslationService, TranslationService>();
builder.Services.AddHttpClient<ICodeTranslationService, CodeTranslationService>();

// Register Services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IGuideService, GuideService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<ISeoTagService, SeoTagService>();
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IStaticPageService, StaticPageService>();
builder.Services.AddScoped<ISiteSettingService, SiteSettingService>();
builder.Services.AddScoped<IVisitorLogService, VisitorLogService>();
builder.Services.AddSingleton<ICacheService, CacheService>();

// Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
    await DbSeeder.SeedAsync(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Custom 404 page
app.UseStatusCodePagesWithReExecute("/404");

app.UseHttpsRedirection();
app.UseResponseCompression();

// Static files
app.UseStaticFiles();

// AppData static files for uploaded images
var appDataPath = Path.Combine(builder.Environment.ContentRootPath, 
    builder.Configuration["AppSettings:AppDataPath"] ?? "AppData");
if (!Directory.Exists(appDataPath))
{
    Directory.CreateDirectory(appDataPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(appDataPath),
    RequestPath = "/appdata"
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Visitor Tracking (after auth, only for authenticated requests to exclude admin)
app.UseVisitorTracking();

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
