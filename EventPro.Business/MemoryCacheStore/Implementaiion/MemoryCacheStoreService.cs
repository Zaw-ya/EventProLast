using EventPro.Business.MemoryCacheStore.Interface;
using StackExchange.Redis;
using System.Runtime.Caching;
using System.Text.Json;

namespace EventPro.Business.MemoryCacheStore.Implementaiion
{
    public class MemoryCacheStoreService : IMemoryCacheStoreService
    {
        private static IDatabase _database;
        public MemoryCacheStoreService(IConnectionMultiplexer redis)
        {
            try
            {
                _database = redis.GetDatabase();
            }
            catch (Exception ex)
            {
            }
        }
        public void delete(string Key)
        {
            if(_database.KeyExists(Key))
                _database.KeyDelete(Key);

        }

        public int Retrieve(string Key)
        {
            try
            {
                var value = _database.StringGet(Key);
                if (!string.IsNullOrEmpty(value))
                {
                    return JsonSerializer.Deserialize<int>(value);
                }

            }
            catch (Exception ex)
            {
            }
            return 0;
        }

        public void save(string key, int value, TimeSpan? expiration = null)
        {
          _database.StringSet(key , JsonSerializer.Serialize(value));
        }

        public bool IsExist(string key)
        {
            if (_database.KeyExists(key))
                return true;
            return false;
        }
    }
}
