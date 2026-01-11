using AlgoritmaUzmani.Services.Interfaces;

namespace AlgoritmaUzmani.Services;

public class ImageService : IImageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly string _appDataPath;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public ImageService(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
        _appDataPath = Path.Combine(_environment.ContentRootPath, 
            _configuration["AppSettings:AppDataPath"] ?? "AppData");
        
        // Ensure AppData directory exists
        if (!Directory.Exists(_appDataPath))
        {
            Directory.CreateDirectory(_appDataPath);
        }
    }

    public async Task<string> SaveImageAsync(IFormFile file, string folder = "images")
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");

        if (!IsValidImage(file))
            throw new ArgumentException("Invalid image file");

        var folderPath = Path.Combine(_appDataPath, folder);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(folderPath, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        // Return relative path
        return $"{folder}/{fileName}";
    }

    public Task<bool> DeleteImageAsync(string path)
    {
        if (string.IsNullOrEmpty(path))
            return Task.FromResult(false);

        var fullPath = Path.Combine(_appDataPath, path);
        
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public string GetImageUrl(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        return $"/appdata/{path}";
    }

    public bool IsValidImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        if (file.Length > MaxFileSize)
            return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return false;

        // Check content type
        var validContentTypes = new[] 
        { 
            "image/jpeg", "image/png", "image/gif", 
            "image/webp", "image/svg+xml" 
        };

        return validContentTypes.Contains(file.ContentType.ToLowerInvariant());
    }
}

