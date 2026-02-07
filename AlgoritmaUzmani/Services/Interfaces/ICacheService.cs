namespace AlgoritmaUzmani.Services.Interfaces;

public interface ICacheService
{
    T? Get<T>(string key);
    Task<T?> GetAsync<T>(string key);
    void Set<T>(string key, T value, TimeSpan? expiration = null);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    void Remove(string key);
    Task RemoveAsync(string key);
    void RemoveByPrefix(string prefix);
    Task RemoveByPrefixAsync(string prefix);
    Task ClearAllAsync();
}
