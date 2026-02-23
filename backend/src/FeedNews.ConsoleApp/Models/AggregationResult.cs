using FeedNews.Domain.Entities;
using FeedNews.Domain.Enums;

namespace FeedNews.ConsoleApp.Models;

/// <summary>
/// Result of news aggregation operation for a specific category
/// </summary>
public class AggregationResult
{
    /// <summary>
    /// News category that was processed
    /// </summary>
    public NewsCategory Category { get; set; }

    /// <summary>
    /// Total number of articles fetched from sources
    /// </summary>
    public int TotalFetched { get; set; }

    /// <summary>
    /// Number of articles that were successfully summarized
    /// </summary>
    public int SummarizedCount { get; set; }

    /// <summary>
    /// Number of top articles selected for sending
    /// </summary>
    public int TopSelected { get; set; }

    /// <summary>
    /// Number of articles successfully sent to Slack
    /// </summary>
    public int SlackSent { get; set; }

    /// <summary>
    /// Number of articles that failed during processing
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// Flag indicating if the operation succeeded
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// List of processed articles
    /// </summary>
    public List<News> ProcessedArticles { get; set; } = [];

    /// <summary>
    /// Timestamp when aggregation was executed
    /// </summary>
    public DateTime ExecutedAt { get; set; }

    /// <summary>
    /// Duration of the aggregation process
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Creates a successful aggregation result
    /// </summary>
    public static AggregationResult Success(
        NewsCategory category,
        int totalFetched,
        int summarizedCount,
        int topSelected,
        int slackSent,
        int failedCount,
        List<News> articles,
        TimeSpan duration)
    {
        return new AggregationResult
        {
            Category = category,
            TotalFetched = totalFetched,
            SummarizedCount = summarizedCount,
            TopSelected = topSelected,
            SlackSent = slackSent,
            FailedCount = failedCount,
            ProcessedArticles = articles,
            IsSuccess = true,
            ExecutedAt = DateTime.UtcNow,
            Duration = duration
        };
    }

    /// <summary>
    /// Creates a failed aggregation result
    /// </summary>
    public static AggregationResult Failure(NewsCategory category, string errorMessage, TimeSpan duration)
    {
        return new AggregationResult
        {
            Category = category,
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ExecutedAt = DateTime.UtcNow,
            Duration = duration
        };
    }

    /// <summary>
    /// Creates a partial success aggregation result (with cached data)
    /// </summary>
    public static AggregationResult PartialSuccess(
        NewsCategory category,
        int totalFetched,
        int summarizedCount,
        int topSelected,
        int slackSent,
        int failedCount,
        List<News> articles,
        TimeSpan duration,
        string warningMessage)
    {
        return new AggregationResult
        {
            Category = category,
            TotalFetched = totalFetched,
            SummarizedCount = summarizedCount,
            TopSelected = topSelected,
            SlackSent = slackSent,
            FailedCount = failedCount,
            ProcessedArticles = articles,
            IsSuccess = false,
            ErrorMessage = warningMessage,
            ExecutedAt = DateTime.UtcNow,
            Duration = duration
        };
    }
}
