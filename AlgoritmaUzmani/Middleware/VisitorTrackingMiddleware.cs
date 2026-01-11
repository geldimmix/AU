using AlgoritmaUzmani.Data;
using AlgoritmaUzmani.Models.Entities;
using System.Text.RegularExpressions;

namespace AlgoritmaUzmani.Middleware;

public class VisitorTrackingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<VisitorTrackingMiddleware> _logger;

    // Exclude these paths from tracking
    private static readonly string[] ExcludedPaths = new[]
    {
        "/admin", "/api", "/sitemap.xml", "/robots.txt", "/favicon.ico",
        "/css", "/js", "/lib", "/appdata", "/_", "/health"
    };

    private static readonly string[] ExcludedExtensions = new[]
    {
        ".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".ico", ".svg", ".woff", ".woff2", ".ttf", ".eot"
    };

    public VisitorTrackingMiddleware(RequestDelegate next, ILogger<VisitorTrackingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        // Skip tracking for excluded paths
        var path = context.Request.Path.Value?.ToLower() ?? "";
        
        if (ShouldSkipTracking(path, context))
        {
            await _next(context);
            return;
        }

        try
        {
            var visitorLog = CreateVisitorLog(context);
            dbContext.VisitorLogs.Add(visitorLog);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log visitor");
        }

        await _next(context);
    }

    private bool ShouldSkipTracking(string path, HttpContext context)
    {
        // Skip if bot
        var userAgent = context.Request.Headers.UserAgent.ToString().ToLower();
        if (IsBot(userAgent))
            return true;

        // Skip excluded paths
        foreach (var excludedPath in ExcludedPaths)
        {
            if (path.StartsWith(excludedPath, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        // Skip excluded extensions
        foreach (var ext in ExcludedExtensions)
        {
            if (path.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private bool IsBot(string userAgent)
    {
        var botPatterns = new[]
        {
            "bot", "crawler", "spider", "googlebot", "bingbot", "slurp",
            "duckduckbot", "baiduspider", "yandexbot", "facebookexternalhit",
            "twitterbot", "rogerbot", "linkedinbot", "embedly", "quora",
            "pinterest", "redditbot", "slackbot", "discordbot", "whatsapp"
        };

        return botPatterns.Any(pattern => userAgent.Contains(pattern));
    }

    private VisitorLog CreateVisitorLog(HttpContext context)
    {
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var (browser, browserVersion) = ParseBrowser(userAgent);
        var os = ParseOperatingSystem(userAgent);
        var deviceType = ParseDeviceType(userAgent);

        // Get or create session
        var sessionId = context.Request.Cookies["visitor_session"];
        var isNewVisitor = string.IsNullOrEmpty(sessionId);
        
        if (isNewVisitor)
        {
            sessionId = Guid.NewGuid().ToString("N");
            context.Response.Cookies.Append("visitor_session", sessionId, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax
            });
        }

        // Get real IP (considering proxies)
        var ipAddress = GetRealIpAddress(context);

        return new VisitorLog
        {
            IpAddress = ipAddress,
            UserAgent = userAgent.Length > 500 ? userAgent[..500] : userAgent,
            Browser = browser,
            BrowserVersion = browserVersion,
            OperatingSystem = os,
            DeviceType = deviceType,
            PageUrl = $"{context.Request.Path}{context.Request.QueryString}",
            Referrer = context.Request.Headers.Referer.ToString(),
            Language = context.Request.Headers.AcceptLanguage.ToString().Split(',').FirstOrDefault()?.Split(';').FirstOrDefault(),
            SessionId = sessionId,
            IsNewVisitor = isNewVisitor,
            VisitedAt = DateTime.UtcNow
        };
    }

    private string GetRealIpAddress(HttpContext context)
    {
        // Check for forwarded headers (reverse proxy)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').First().Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private (string browser, string version) ParseBrowser(string userAgent)
    {
        var patterns = new Dictionary<string, string>
        {
            { "Edg", "Edge" },
            { "OPR", "Opera" },
            { "Opera", "Opera" },
            { "Chrome", "Chrome" },
            { "Safari", "Safari" },
            { "Firefox", "Firefox" },
            { "MSIE", "Internet Explorer" },
            { "Trident", "Internet Explorer" }
        };

        foreach (var pattern in patterns)
        {
            if (userAgent.Contains(pattern.Key))
            {
                var version = ExtractVersion(userAgent, pattern.Key);
                return (pattern.Value, version);
            }
        }

        return ("Unknown", "");
    }

    private string ExtractVersion(string userAgent, string browser)
    {
        try
        {
            var pattern = browser switch
            {
                "Edg" => @"Edg/(\d+\.?\d*)",
                "OPR" => @"OPR/(\d+\.?\d*)",
                "Chrome" => @"Chrome/(\d+\.?\d*)",
                "Safari" => @"Version/(\d+\.?\d*)",
                "Firefox" => @"Firefox/(\d+\.?\d*)",
                "MSIE" => @"MSIE (\d+\.?\d*)",
                "Trident" => @"rv:(\d+\.?\d*)",
                _ => null
            };

            if (pattern != null)
            {
                var match = Regex.Match(userAgent, pattern);
                if (match.Success)
                    return match.Groups[1].Value;
            }
        }
        catch { }

        return "";
    }

    private string ParseOperatingSystem(string userAgent)
    {
        if (userAgent.Contains("Windows NT 10.0")) return "Windows 10/11";
        if (userAgent.Contains("Windows NT 6.3")) return "Windows 8.1";
        if (userAgent.Contains("Windows NT 6.2")) return "Windows 8";
        if (userAgent.Contains("Windows NT 6.1")) return "Windows 7";
        if (userAgent.Contains("Windows")) return "Windows";
        if (userAgent.Contains("Mac OS X")) return "macOS";
        if (userAgent.Contains("Android")) return "Android";
        if (userAgent.Contains("iPhone") || userAgent.Contains("iPad")) return "iOS";
        if (userAgent.Contains("Linux")) return "Linux";
        return "Unknown";
    }

    private string ParseDeviceType(string userAgent)
    {
        if (userAgent.Contains("Mobile") || userAgent.Contains("Android") && !userAgent.Contains("Tablet"))
            return "Mobile";
        if (userAgent.Contains("Tablet") || userAgent.Contains("iPad"))
            return "Tablet";
        return "Desktop";
    }
}

public static class VisitorTrackingMiddlewareExtensions
{
    public static IApplicationBuilder UseVisitorTracking(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<VisitorTrackingMiddleware>();
    }
}

