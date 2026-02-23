using FeedNews.Application.Features.News.Commands;
using FeedNews.Application.Features.News.Queries;
using FeedNews.ConsoleApp.Models;
using FeedNews.Domain.Entities;
using FeedNews.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FeedNews.ConsoleApp.Services;

/// <summary>
/// Orchestrates the complete news aggregation pipeline for a specific category.
/// Coordinates fetching, summarizing, ranking, and sending articles to Slack.
/// </summary>
public interface INewsAggregationOrchestrator
{
    /// <summary>
    /// Executes the complete aggregation pipeline for a specific news category.
    /// </summary>
    /// <param name="category">The news category to aggregate</param>
    /// <returns>Result containing aggregation statistics and processed articles</returns>
    Task<AggregationResult> ExecuteAggregationAsync(NewsCategory category);
}

/// <summary>
/// Implementation of the news aggregation orchestrator
/// </summary>
public class NewsAggregationOrchestrator : INewsAggregationOrchestrator
{
    private readonly IMediator _mediator;
    private readonly ILogger<NewsAggregationOrchestrator> _logger;

    public NewsAggregationOrchestrator(IMediator mediator, ILogger<NewsAggregationOrchestrator> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Executes the aggregation pipeline:
    /// 1. Fetch news articles from sources
    /// 2. Generate AI summaries for each article
    /// 3. Rank and select top articles
    /// 4. Send selected articles to Slack
    /// </summary>
    public async Task<AggregationResult> ExecuteAggregationAsync(NewsCategory category)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("🔄 Starting aggregation for {Category}", category);

            // Step 1: Fetch articles
            var fetchedArticles = await FetchArticlesAsync(category);
            if (fetchedArticles.Count == 0)
            {
                _logger.LogWarning("⚠️ No articles fetched for {Category}, attempting to retrieve from cache", category);
                return await HandleEmptyFetchWithCache(category, stopwatch.Elapsed);
            }

            _logger.LogInformation("✅ Fetched {Count} articles for {Category}", fetchedArticles.Count, category);

            // Step 2: Generate summaries
            var summarizedCount = await GenerateSummariesAsync(fetchedArticles, category);
            _logger.LogInformation("✅ Generated summaries for {Count}/{Total} articles", summarizedCount, fetchedArticles.Count);

            // Step 3: Rank and select top articles
            var topArticles = await RankAndSelectTopArticlesAsync(fetchedArticles, category);
            _logger.LogInformation("✅ Selected top {Count} articles", topArticles.Count);

            // Step 4: Send to Slack
            var slackSentCount = await SendToSlackAsync(topArticles, category);
            _logger.LogInformation("✅ Sent {Count} articles to Slack", slackSentCount);

            stopwatch.Stop();

            return AggregationResult.Success(
                category,
                fetchedArticles.Count,
                summarizedCount,
                topArticles.Count,
                slackSentCount,
                fetchedArticles.Count - summarizedCount,
                topArticles,
                stopwatch.Elapsed
            );
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ Error during aggregation for {Category}: {Message}", category, ex.Message);
            return AggregationResult.Failure(category, $"Aggregation failed: {ex.Message}", stopwatch.Elapsed);
        }
    }

    /// <summary>
    /// Fetches articles from news sources for the specified category
    /// </summary>
    private async Task<List<News>> FetchArticlesAsync(NewsCategory category)
    {
        try
        {
            var command = new FetchNewsCommand(category);
            var articles = await _mediator.Send(command);
            return articles ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Exception while fetching news for {Category}", category);
            return [];
        }
    }

    /// <summary>
    /// Generates AI summaries for articles
    /// </summary>
    private async Task<int> GenerateSummariesAsync(List<News> articles, NewsCategory category)
    {
        int successCount = 0;

        foreach (var article in articles)
        {
            try
            {
                var command = new GenerateSummaryCommand(article);
                await _mediator.Send(command);
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to generate summary for article {ArticleId}", article.Id);
                // Continue with next article
            }
        }

        return successCount;
    }

    /// <summary>
    /// Ranks articles and selects top N articles
    /// </summary>
    private async Task<List<News>> RankAndSelectTopArticlesAsync(List<News> articles, NewsCategory category)
    {
        try
        {
            var query = new RankAndSelectTopNewsQuery(category, articles);
            var topArticles = await _mediator.Send(query);
            return topArticles ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Error ranking articles for {Category}, returning all articles", category);
            return articles;
        }
    }

    /// <summary>
    /// Sends articles to Slack
    /// Always sends if articles qualify (as per user requirement)
    /// </summary>
    private async Task<int> SendToSlackAsync(List<News> articles, NewsCategory category)
    {
        try
        {
            if (articles.Count == 0)
                return 0;

            var command = new SendNewsToSlackCommand(articles);
            var result = await _mediator.Send(command);

            int sentCount = result ? articles.Count : 0;
            _logger.LogDebug("✅ Sent {Count} articles to Slack", sentCount);
            return sentCount;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to send articles to Slack");
            // Continue even if Slack send fails
            return 0;
        }
    }

    /// <summary>
    /// Handles the case where no articles were fetched by attempting to use cached articles
    /// </summary>
    private async Task<AggregationResult> HandleEmptyFetchWithCache(NewsCategory category, TimeSpan elapsedTime)
    {
        try
        {
            // Try to get cached articles from database
            var cachedArticles = await GetCachedArticlesAsync(category);

            if (cachedArticles.Count > 0)
            {
                _logger.LogInformation("📦 Using {Count} cached articles for {Category}", cachedArticles.Count, category);

                // Generate summaries for cached articles if needed
                var summarizedCount = await GenerateSummariesAsync(cachedArticles, category);

                // Rank and select
                var topArticles = await RankAndSelectTopArticlesAsync(cachedArticles, category);

                // Send to Slack
                var slackSentCount = await SendToSlackAsync(topArticles, category);

                return AggregationResult.PartialSuccess(
                    category,
                    cachedArticles.Count,
                    summarizedCount,
                    topArticles.Count,
                    slackSentCount,
                    cachedArticles.Count - summarizedCount,
                    topArticles,
                    elapsedTime,
                    "Fetch failed, using cached articles from previous aggregation"
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to retrieve cached articles for {Category}", category);
        }

        return AggregationResult.Failure(
            category,
            "Unable to fetch articles and no cache available",
            elapsedTime
        );
    }

    /// <summary>
    /// Retrieves previously fetched articles from database cache
    /// Used when fresh fetch fails
    /// </summary>
    private async Task<List<News>> GetCachedArticlesAsync(NewsCategory category)
    {
        try
        {
            // For now, return empty list as we don't have a separate cache query
            // In a real scenario, you would query DB directly for recent articles of this category
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to get cached articles for {Category}", category);
            return [];
        }
    }
}
