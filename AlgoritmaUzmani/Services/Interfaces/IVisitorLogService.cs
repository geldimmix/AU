using AlgoritmaUzmani.Models.Entities;

namespace AlgoritmaUzmani.Services.Interfaces;

public interface IVisitorLogService
{
    Task<List<VisitorLog>> GetRecentAsync(int count = 100);
    Task<List<VisitorLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 50);
    Task<int> GetTotalCountAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<int> GetUniqueVisitorsCountAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<Dictionary<string, int>> GetBrowserStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<Dictionary<string, int>> GetDeviceStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<Dictionary<string, int>> GetOsStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<Dictionary<string, int>> GetTopPagesAsync(int count = 10, DateTime? startDate = null, DateTime? endDate = null);
    Task<Dictionary<DateTime, int>> GetDailyVisitsAsync(int days = 30);
    Task CleanupOldLogsAsync(int daysToKeep = 90);
}

