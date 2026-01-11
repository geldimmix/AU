using Microsoft.AspNetCore.Http;

namespace AlgoritmaUzmani.Services.Interfaces;

public interface IImageService
{
    Task<string> SaveImageAsync(IFormFile file, string folder = "images");
    Task<bool> DeleteImageAsync(string path);
    string GetImageUrl(string path);
    bool IsValidImage(IFormFile file);
}

