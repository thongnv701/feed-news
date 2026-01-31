using FeedNews.Domain.Enums;

namespace FeedNews.Domain.Entities;

/// <summary>
/// Domain entity representing a news article from Reuters or VNExpress.
/// Stores title, summary, source, category, and tracking information.
/// </summary>
public class News
{
    /// <summary>
    /// Unique identifier for the news article.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Article title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// URL to the original article.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// AI-generated summary of the article (200-500 characters).
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// News source (Reuters or VNExpress).
    /// </summary>
    public NewsSource Source { get; set; }

    /// <summary>
    /// News category (Business, Technology, or World).
    /// </summary>
    public NewsCategory Category { get; set; }

    /// <summary>
    /// Original publication date of the article.
    /// </summary>
    public DateTime PublishedDate { get; set; }

    /// <summary>
    /// Date and time when the article was fetched.
    /// </summary>
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Ranking score for sorting and filtering (higher = more important).
    /// Default: 0. Could be used for future ranking algorithms.
    /// </summary>
    public decimal RankingScore { get; set; } = 0m;

    /// <summary>
    /// Date and time when the article was sent to Slack (nullable if not sent yet).
    /// </summary>
    public DateTime? SlackSentAt { get; set; }

    /// <summary>
    /// Record creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Record last update timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
