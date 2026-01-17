using AlgoritmaUzmani.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Text.Json;

namespace AlgoritmaUzmani.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, byte> _keys = new();
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromHours(1);

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        if (_cache.TryGetValue(key, out T? value))
        {
            return Task.FromResult(value);
        }
        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration
        };

        options.RegisterPostEvictionCallback((evictedKey, _, _, _) =>
        {
            _keys.TryRemove(evictedKey.ToString()!, out _);
        });

        _cache.Set(key, value, options);
        _keys.TryAdd(key, 0);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        _keys.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix)
    {
        var keysToRemove = _keys.Keys
            .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            _keys.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    public Task ClearAllAsync()
    {
        foreach (var key in _keys.Keys)
        {
            _cache.Remove(key);
        }
        _keys.Clear();
        return Task.CompletedTask;
    }
}



