namespace Application.Interfaces
{
    /// <summary>
    /// Interface for distributed caching abstraction.
    /// Provides methods to get, set, and remove cached items.
    /// Supports both synchronous and asynchronous operations.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Gets a cached item by key.
        /// </summary>
        /// <typeparam name="T">The type of the cached object.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <returns>The cached object, or null if not found.</returns>
        Task<T?> GetAsync<T>(string key) where T : class;

        /// <summary>
        /// Sets a cached item with a default expiration time.
        /// </summary>
        /// <typeparam name="T">The type of the object to cache.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The object to cache.</param>
        /// <returns>A completed task.</returns>
        Task SetAsync<T>(string key, T value) where T : class;

        /// <summary>
        /// Sets a cached item with a custom expiration time.
        /// </summary>
        /// <typeparam name="T">The type of the object to cache.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The object to cache.</param>
        /// <param name="expirationMinutes">The cache expiration time in minutes.</param>
        /// <returns>A completed task.</returns>
        Task SetAsync<T>(string key, T value, int expirationMinutes) where T : class;

        /// <summary>
        /// Removes a cached item.
        /// </summary>
        /// <param name="key">The cache key to remove.</param>
        /// <returns>A completed task.</returns>
        Task RemoveAsync(string key);

        /// <summary>
        /// Removes multiple cached items by key pattern.
        /// </summary>
        /// <param name="keyPattern">The key pattern (e.g., "org_tasks:*").</param>
        /// <returns>A completed task.</returns>
        Task RemoveByPatternAsync(string keyPattern);
    }
}
