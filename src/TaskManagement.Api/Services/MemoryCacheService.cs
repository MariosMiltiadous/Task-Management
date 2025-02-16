using Microsoft.Extensions.Caching.Memory;

namespace TaskManagement.Api.Services
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            return _cache.TryGetValue(key, out T value) ? value : default;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            _cache.Set(key, value, expiration);
        }

        public async Task RemoveAsync(string key)
        {
            _cache.Remove(key);
        }
    }
}
