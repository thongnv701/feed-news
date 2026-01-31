namespace FeedNews.Application.Common.Services;

public interface ICacheService
{
    Task SetCacheResponseAsync(string key, object? response, TimeSpan timeout);
    Task<string?> GetCachedResponseAsync(string key);
    Task RemoveCacheResponseAsync(string pattern);
    Task<int> GenerateAndSaveUniqueSixDigitCodeAsync(string cacheKeyPrefix, TimeSpan timeout);
}