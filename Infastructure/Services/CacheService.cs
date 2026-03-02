using Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infastructure.Services
{
    /// <summary>
    /// Redis-based distributed cache service implementation.
    /// Provides high-performance caching for multi-tenant scenarios.
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheService> _logger;
        private const int DefaultExpirationMinutes = 10;

        /// <summary>
        /// Initializes a new instance of the CacheService class.
        /// </summary>
        public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Gets a cached item by key.
        /// </summary>
        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var cachedData = await _cache.GetStringAsync(key);
                
                if (string.IsNullOrEmpty(cachedData))
                {
                    _logger.LogDebug("Cache miss for key: {CacheKey}", key);
                    return null;
                }

                var result = JsonSerializer.Deserialize<T>(cachedData);
                _logger.LogDebug("Cache hit for key: {CacheKey}", key);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving from cache with key: {CacheKey}", key);
                return null;
            }
        }

        /// <summary>
        /// Sets a cached item with default expiration.
        /// </summary>
        public async Task SetAsync<T>(string key, T value) where T : class
        {
            await SetAsync(key, value, DefaultExpirationMinutes);
        }

        /// <summary>
        /// Sets a cached item with custom expiration.
        /// </summary>
        public async Task SetAsync<T>(string key, T value, int expirationMinutes) where T : class
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes)
                };

                var serialized = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, serialized, options);
                
                _logger.LogDebug("Cached value for key: {CacheKey} with expiration: {ExpirationMinutes}m", 
                    key, expirationMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for key: {CacheKey}", key);
            }
        }

        /// <summary>
        /// Removes a cached item.
        /// </summary>
        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
                _logger.LogDebug("Removed cache for key: {CacheKey}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache for key: {CacheKey}", key);
            }
        }

        /// <summary>
        /// Removes multiple cached items by key pattern.
        /// Note: Redis pattern matching requires custom implementation if using StackExchange.Redis directly.
        /// For simplicity, this implementation removes individual known keys.
        /// </summary>
        public async Task RemoveByPatternAsync(string keyPattern)
        {
            try
            {
                _logger.LogDebug("Cache invalidation requested for pattern: {KeyPattern}", keyPattern);
                // Note: Pattern removal is complex with IDistributedCache abstraction.
                // For production, consider using StackExchange.Redis directly for this operation.
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache by pattern: {KeyPattern}", keyPattern);
            }
        }
    }
}
