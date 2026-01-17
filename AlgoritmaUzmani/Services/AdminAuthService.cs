using AlgoritmaUzmani.Data;
using AlgoritmaUzmani.Models.Entities;
using AlgoritmaUzmani.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlgoritmaUzmani.Services;

public class AdminAuthService : IAdminAuthService
{
    private readonly ApplicationDbContext _context;

    public AdminAuthService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminUser?> ValidateCredentialsAsync(string username, string password)
    {
        var user = await _context.AdminUsers
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

        if (user == null)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        return user;
    }

    public async Task<AdminUser?> GetByIdAsync(int id)
    {
        return await _context.AdminUsers.FindAsync(id);
    }

    public async Task UpdateLastLoginAsync(int userId)
    {
        await _context.AdminUsers
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.LastLoginAt, DateTime.UtcNow));
    }
}



