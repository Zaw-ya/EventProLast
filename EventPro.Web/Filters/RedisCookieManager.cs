using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using System;

namespace EventPro.Web.Filters
{
    public class RedisCookieManager : ICookieManager
    {
        private readonly IDistributedCache _cache;
        public RedisCookieManager(IDistributedCache cache)
        {
            _cache = cache;
        }
        string? ICookieManager.GetRequestCookie(HttpContext context, string key)
        {
            var result = _cache.GetString(key);
            return result;
        }
        void ICookieManager.AppendResponseCookie(HttpContext context, string key, string? value, CookieOptions options)
        {
            //var redisKey = $"{sessionId}:cookies:{key}";
            var optionsWithExpiry = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12)
            };
            _cache.SetString(key, value, optionsWithExpiry);
        }
        void ICookieManager.DeleteCookie(HttpContext context, string key, CookieOptions options)
        {
            var redisKey = key;
            _cache.Remove(redisKey);
        }
    }
}
