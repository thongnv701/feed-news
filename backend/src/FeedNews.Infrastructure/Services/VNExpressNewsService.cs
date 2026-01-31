using System.Xml.Linq;
using FeedNews.Application.Contracts.Services;
using FeedNews.Domain.Entities;
using FeedNews.Domain.Enums;
using FeedNews.Application.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;

namespace FeedNews.Infrastructure.Services;

public class VNExpressNewsService : IVNExpressNewsService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly NewsFeedsConfiguration _feedsConfig;
    private readonly ILogger<VNExpressNewsService> _logger;

    public VNExpressNewsService(
        IHttpClientFactory httpClientFactory,
        IOptions<NewsFeedsConfiguration> feedsConfig,
        ILogger<VNExpressNewsService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _feedsConfig = feedsConfig.Value;
        _logger = logger;
    }

    public async Task<List<News>> FetchNewsByCategoryAsync(NewsCategory category)
    {
        _logger.LogInformation("Fetching VNExpress news for category: {Category}", category);
        
        var categoryName = category.ToString();
        
        if (!_feedsConfig.VNExpress.RssFeedUrls.ContainsKey(categoryName))
        {
            _logger.LogWarning("No VNExpress RSS URL configured for category: {Category}", categoryName);
            return new List<News>();
        }

        var rssUrl = _feedsConfig.VNExpress.RssFeedUrls[categoryName];

        try
        {
            var httpClient = _httpClientFactory.CreateClient("VNExpressClient");
            
            var policy = Policy
                .Handle<HttpRequestException>()
                .Or<OperationCanceledException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timespan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            "Retry {RetryCount} for VNExpress feed after {DelayMs}ms. Exception: {Exception}",
                            retryCount,
                            timespan.TotalMilliseconds,
                            exception.Message);
                    });

            var content = await policy.ExecuteAsync(async () =>
            {
                using var response = await httpClient.GetAsync(rssUrl);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            });

            var newsList = ParseRssFeed(content, NewsSource.VNExpress, category);

            _logger.LogInformation("Successfully fetched {Count} articles from VNExpress for category {Category}",
                newsList.Count, categoryName);

            return newsList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching VNExpress news for category {Category}", categoryName);
            return new List<News>();
        }
    }

    private List<News> ParseRssFeed(string rssContent, NewsSource source, NewsCategory category)
    {
        var newsList = new List<News>();

        try
        {
            var doc = XDocument.Parse(rssContent);
            var items = doc.Descendants("item");

            foreach (var item in items.Take(20))
            {
                var title = item.Element("title")?.Value ?? "Untitled";
                var link = item.Element("link")?.Value ?? string.Empty;
                var pubDate = item.Element("pubDate")?.Value;

                if (string.IsNullOrWhiteSpace(link))
                    continue;

                var publishedDate = DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(pubDate) && DateTime.TryParse(pubDate, out var parsed))
                {
                    publishedDate = parsed.ToUniversalTime();
                }

                newsList.Add(new News
                {
                    Title = title,
                    Url = link,
                    Source = source,
                    Category = category,
                    PublishedDate = publishedDate,
                    FetchedAt = DateTime.UtcNow,
                    Summary = string.Empty
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing RSS feed from {Source}", source);
        }

        return newsList;
    }
}
