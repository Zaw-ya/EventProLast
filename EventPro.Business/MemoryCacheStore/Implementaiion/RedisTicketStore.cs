using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;

public class RedisTicketStore : ITicketStore
{
    private readonly IDatabase _database;
    private const string KeyPrefix = "xxx";

    public RedisTicketStore(string redisConnectionString)
    {
        var redis = ConnectionMultiplexer.Connect(redisConnectionString);
        _database = redis.GetDatabase();
    }

    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(KeyPrefix + key);
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var data = SerializeToBytes(ticket);
        await _database.StringSetAsync(KeyPrefix + key, data, TimeSpan.FromHours(2));
    }

    public async Task<AuthenticationTicket> RetrieveAsync(string key)
    {
        var data = await _database.StringGetAsync(KeyPrefix + key);
        return data.HasValue ? DeserializeFromBytes(data) : null;
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = Guid.NewGuid().ToString("N");
        await RenewAsync(key, ticket);
        return key;
    }

    private byte[] SerializeToBytes(AuthenticationTicket source)
    {
        return TicketSerializer.Default.Serialize(source);
    }

    private AuthenticationTicket DeserializeFromBytes(byte[] source)
    {
        return TicketSerializer.Default.Deserialize(source);
    }
}

