using Microsoft.Extensions.Caching.Memory;
using AlgoritmaUzmani.Services.Interfaces;

namespace AlgoritmaUzmani.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly HashSet<string> _keys = new();
    private readonly object _lock = new();

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public T? Get<T>(string key)
    {
        return _cache.TryGetValue(key, out T? value) ? value : default;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        return Task.FromResult(Get<T>(key));
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions();
        
        if (expiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration;
        }
        else
        {
            options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
        }

        lock (_lock)
        {
            _keys.Add(key);
        }

        _cache.Set(key, value, options);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        Set(key, value, expiration);
        return Task.CompletedTask;
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
        
        lock (_lock)
        {
            _keys.Remove(key);
        }
    }

    public Task RemoveAsync(string key)
    {
        Remove(key);
        return Task.CompletedTask;
    }

    public void RemoveByPrefix(string prefix)
    {
        lock (_lock)
        {
            var keysToRemove = _keys.Where(k => k.StartsWith(prefix)).ToList();
            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
                _keys.Remove(key);
            }
        }
    }

    public Task RemoveByPrefixAsync(string prefix)
    {
        RemoveByPrefix(prefix);
        return Task.CompletedTask;
    }

    public Task ClearAllAsync()
    {
        lock (_lock)
        {
            foreach (var key in _keys.ToList())
            {
                _cache.Remove(key);
            }
            _keys.Clear();
        }
        return Task.CompletedTask;
    }
}
