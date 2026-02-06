using AlgoritmaUzmani.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Text.Json;

namespace AlgoritmaUzmani.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, byte> _keys = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
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

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
    {
        // İlk kontrol - cache'te varsa direkt dön
        if (_cache.TryGetValue(key, out T? cached) && cached != null)
        {
            return cached;
        }

        // Bu key için bir lock al (veya var olanı kullan)
        var keyLock = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

        await keyLock.WaitAsync();
        try
        {
            // Double-check: lock aldıktan sonra tekrar kontrol et
            // (başka bir thread bu arada doldurmuş olabilir)
            if (_cache.TryGetValue(key, out cached) && cached != null)
            {
                return cached;
            }

            // Cache'te yok, factory'yi çağır
            var value = await factory();

            if (value != null)
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
            }

            return value!;
        }
        finally
        {
            keyLock.Release();
            
            // Lock'u temizle (memory leak önleme)
            // Ancak sadece bekleyen yoksa temizle
            if (keyLock.CurrentCount == 1)
            {
                _locks.TryRemove(key, out _);
            }
        }
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





