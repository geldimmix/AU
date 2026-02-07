using AlgoritmaUzmani.Data;
using AlgoritmaUzmani.Models.Entities;
using AlgoritmaUzmani.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlgoritmaUzmani.Services;

public class VisitorLogService : IVisitorLogService
{
    private readonly ApplicationDbContext _context;

    public VisitorLogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogVisitAsync(string path, string ipAddress, string userAgent, string? referer = null)
    {
        var log = new VisitorLog
        {
            PageUrl = path,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Referrer = referer,
            VisitedAt = DateTime.UtcNow,
            Browser = ParseBrowser(userAgent),
            OperatingSystem = ParseOS(userAgent),
            DeviceType = ParseDeviceType(userAgent),
            SessionId = Guid.NewGuid().ToString("N")[..20]
        };

        _context.VisitorLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    private static string ParseBrowser(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown";
        if (userAgent.Contains("Firefox")) return "Firefox";
        if (userAgent.Contains("Edg")) return "Edge";
        if (userAgent.Contains("Chrome")) return "Chrome";
        if (userAgent.Contains("Safari")) return "Safari";
        if (userAgent.Contains("Opera") || userAgent.Contains("OPR")) return "Opera";
        return "Other";
    }

    private static string ParseOS(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown";
        if (userAgent.Contains("Windows")) return "Windows";
        if (userAgent.Contains("Mac OS")) return "macOS";
        if (userAgent.Contains("Linux")) return "Linux";
        if (userAgent.Contains("Android")) return "Android";
        if (userAgent.Contains("iOS") || userAgent.Contains("iPhone") || userAgent.Contains("iPad")) return "iOS";
        return "Other";
    }

    private static string ParseDeviceType(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown";
        if (userAgent.Contains("Mobile") || userAgent.Contains("Android") || userAgent.Contains("iPhone")) return "Mobile";
        if (userAgent.Contains("Tablet") || userAgent.Contains("iPad")) return "Tablet";
        return "Desktop";
    }

    public async Task<List<VisitorLog>> GetRecentAsync(int count = 100)
    {
        return await _context.VisitorLogs
            .OrderByDescending(v => v.VisitedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<VisitorLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 50)
    {
        return await _context.VisitorLogs
            .Where(v => v.VisitedAt >= startDate && v.VisitedAt <= endDate)
            .OrderByDescending(v => v.VisitedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.VisitorLogs.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(v => v.VisitedAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(v => v.VisitedAt <= endDate.Value);

        return await query.CountAsync();
    }

    public async Task<int> GetUniqueVisitorsCountAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.VisitorLogs.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(v => v.VisitedAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(v => v.VisitedAt <= endDate.Value);

        return await query.Select(v => v.SessionId).Distinct().CountAsync();
    }

    public async Task<Dictionary<string, int>> GetBrowserStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.VisitorLogs.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(v => v.VisitedAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(v => v.VisitedAt <= endDate.Value);

        return await query
            .GroupBy(v => v.Browser ?? "Unknown")
            .Select(g => new { Browser = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToDictionaryAsync(x => x.Browser, x => x.Count);
    }

    public async Task<Dictionary<string, int>> GetDeviceStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.VisitorLogs.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(v => v.VisitedAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(v => v.VisitedAt <= endDate.Value);

        return await query
            .GroupBy(v => v.DeviceType ?? "Unknown")
            .Select(g => new { Device = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToDictionaryAsync(x => x.Device, x => x.Count);
    }

    public async Task<Dictionary<string, int>> GetOsStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.VisitorLogs.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(v => v.VisitedAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(v => v.VisitedAt <= endDate.Value);

        return await query
            .GroupBy(v => v.OperatingSystem ?? "Unknown")
            .Select(g => new { OS = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToDictionaryAsync(x => x.OS, x => x.Count);
    }

    public async Task<Dictionary<string, int>> GetTopPagesAsync(int count = 10, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.VisitorLogs.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(v => v.VisitedAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(v => v.VisitedAt <= endDate.Value);

        return await query
            .GroupBy(v => v.PageUrl ?? "/")
            .Select(g => new { Page = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(count)
            .ToDictionaryAsync(x => x.Page, x => x.Count);
    }

    public async Task<Dictionary<DateTime, int>> GetDailyVisitsAsync(int days = 30)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days + 1);

        var visits = await _context.VisitorLogs
            .Where(v => v.VisitedAt >= startDate)
            .GroupBy(v => v.VisitedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync();

        // Fill in missing days with 0
        var result = new Dictionary<DateTime, int>();
        for (var date = startDate; date <= DateTime.UtcNow.Date; date = date.AddDays(1))
        {
            var visit = visits.FirstOrDefault(v => v.Date == date);
            result[date] = visit?.Count ?? 0;
        }

        return result;
    }

    public async Task CleanupOldLogsAsync(int daysToKeep = 90)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
        
        await _context.VisitorLogs
            .Where(v => v.VisitedAt < cutoffDate)
            .ExecuteDeleteAsync();
    }
}






