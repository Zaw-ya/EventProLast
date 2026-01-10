namespace EventPro.Business.MemoryCacheStore.Interface
{
    public interface IMemoryCacheStoreService
    {
        public void save(string key, int value, TimeSpan? expiration = null);
        public int Retrieve(string Key);
        public void delete(string Key);
        public bool IsExist(string key);
    }
}
