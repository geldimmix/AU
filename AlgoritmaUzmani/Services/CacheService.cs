using AlgoritmaUzmani.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace AlgoritmaUzmani.Services;

/// <summary>
/// Basitleştirilmiş cache servisi - Entity Framework nesneleri için cache KULLANILMIYOR
/// IMemoryCache sadece basit değerler için kullanılmalı (string, int, vs.)
/// </summary>
public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        // Cache devre dışı - her zaman null dön, factory çalışsın
        return Task.FromResult<T?>(null);
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
    {
        // Cache devre dışı - direkt factory'yi çağır
        return await factory();
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        // Cache devre dışı - hiçbir şey yapma
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix)
    {
        return Task.CompletedTask;
    }

    public Task ClearAllAsync()
    {
        return Task.CompletedTask;
    }
}
