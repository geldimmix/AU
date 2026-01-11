using AlgoritmaUzmani.Models.Entities;

namespace AlgoritmaUzmani.Services.Interfaces;

public interface IAdminAuthService
{
    Task<AdminUser?> ValidateCredentialsAsync(string username, string password);
    Task<AdminUser?> GetByIdAsync(int id);
    Task UpdateLastLoginAsync(int userId);
}

