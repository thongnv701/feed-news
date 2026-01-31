using System.Net;
using FeedNews.Application.Common.Services;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;

namespace FeedNews.Infrastructure.Services;

public class CacheService : ICacheService, IBaseService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDistributedCache _distributedCache;

    public CacheService(IDistributedCache distributedCache, IConnectionMultiplexer connectionMultiplexer)
    {
        _distributedCache = distributedCache;
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task SetCacheResponseAsync(string key, object? response, TimeSpan timeout)
    {
        if (response == null) return;

        string serializerResponse = JsonConvert.SerializeObject(response, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });
        await _distributedCache.SetStringAsync(key, serializerResponse, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = timeout
        });
    }

    public async Task<string?> GetCachedResponseAsync(string key)
    {
        string? cacheResponse = await _distributedCache.GetStringAsync(key);
        return string.IsNullOrEmpty(cacheResponse) ? null : cacheResponse;
    }

    public async Task RemoveCacheResponseAsync(string pattern)
    {
        foreach (string key in GetKeyAsync(pattern + "*")) await _distributedCache.RemoveAsync(key);
    }

    public async Task<int> GenerateAndSaveUniqueSixDigitCodeAsync(string cacheKeyPrefix, TimeSpan timeout)
    {
        Random random = new();
        int code;
        code = random.Next(100000, 1000000);

        await SetCacheResponseAsync(cacheKeyPrefix, code, timeout);
        return code;
    }

    private IEnumerable<string> GetKeyAsync(string pattern)
    {
        foreach (EndPoint endPoint in _connectionMultiplexer.GetEndPoints())
        {
            IServer server = _connectionMultiplexer.GetServer(endPoint);
            foreach (RedisKey key in server.Keys(pattern: pattern)) yield return key.ToString();
        }
    }
}