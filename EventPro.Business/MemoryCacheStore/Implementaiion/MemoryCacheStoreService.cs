using EventPro.Business.MemoryCacheStore.Interface;
using StackExchange.Redis;
using System.Text.Json;

namespace EventPro.Business.MemoryCacheStore.Implementaiion
{
    /// <summary>
    /// Provides a Redis-based cache store implementation using StackExchange.Redis.
    ///
    /// This service is responsible for:
    /// - Saving integer values into Redis cache
    /// - Retrieving cached values by key
    /// - Checking whether a key exists
    /// - Deleting cached values
    ///
    /// It acts as a lightweight caching layer to improve application performance
    /// and reduce repeated or expensive operations such as database calls.
    ///
    /// The service uses <see cref="IConnectionMultiplexer"/> to access Redis
    /// and implements <see cref="IMemoryCacheStoreService"/> for abstraction.
    /// </summary>
    public class MemoryCacheStoreService : IMemoryCacheStoreService
    {
        private static IDatabase _database;

        /// <summary>
        /// Initializes a new instance of <see cref="MemoryCacheStoreService"/>
        /// and retrieves the Redis database instance.
        /// </summary>
        /// <param name="redis">Redis connection multiplexer</param>
        public MemoryCacheStoreService(IConnectionMultiplexer redis)
        {
            try
            {
                _database = redis.GetDatabase();
            }
            catch (Exception)
            {
                // Redis connection failure is silently ignored
                // Cache will be unavailable but application will continue running
            }
        }

        /// <summary>
        /// Deletes a cached value from Redis by its key.
        /// </summary>
        /// <param name="Key">Cache key</param>
        public void delete(string Key)
        {
            if (_database != null && _database.KeyExists(Key))
                _database.KeyDelete(Key);
        }

        /// <summary>
        /// Retrieves an integer value from Redis cache.
        /// Returns 0 if the key does not exist or cache is unavailable.
        /// </summary>
        /// <param name="Key">Cache key</param>
        /// <returns>Cached integer value or 0</returns>
        public int Retrieve(string Key)
        {
            try
            {
                if (_database == null) return 0;

                var value = _database.StringGet(Key);
                if (!string.IsNullOrEmpty(value))
                {
                    return JsonSerializer.Deserialize<int>(value!);
                }
            }
            catch (Exception)
            {
                // Deserialization or Redis read failure
            }

            return 0;
        }

        /// <summary>
        /// Saves an integer value into Redis cache.
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="value">Value to cache</param>
        /// <param name="expiration">Optional expiration time</param>
        public void save(string key, int value, TimeSpan? expiration = null)
        {
            if (_database != null)
                _database.StringSet(key, JsonSerializer.Serialize(value));
        }

        /// <summary>
        /// Checks whether a specific key exists in Redis cache.
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <returns>True if key exists, otherwise false</returns>
        public bool IsExist(string key)
        {
            if (_database != null && _database.KeyExists(key))
                return true;

            return false;
        }
    }
}
