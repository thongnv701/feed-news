using System.Text.Json;
using FeedNews.Application.Common.Repositories;
using FeedNews.Application.Contracts.Services;
using FeedNews.Domain.Entities;
using FeedNews.Application.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FeedNews.Infrastructure.Services;

public class SlackNotificationService : ISlackNotificationService
{
    private readonly SlackSettings _slackSettings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IArticleAnalysisResultRepository _analysisResultRepository;
    private readonly ILogger<SlackNotificationService> _logger;

    public SlackNotificationService(
        IOptions<SlackSettings> slackSettings,
        IHttpClientFactory httpClientFactory,
        IArticleAnalysisResultRepository analysisResultRepository,
        ILogger<SlackNotificationService> logger)
    {
        _slackSettings = slackSettings.Value;
        _httpClientFactory = httpClientFactory;
        _analysisResultRepository = analysisResultRepository;
        _logger = logger;
    }

    public async Task<bool> SendNewsToSlackAsync(List<News> articles)
    {
        // Use Webhook URL if configured, otherwise fallback to Bot Token
        var webhookUrl = _slackSettings.WebhookUrl;
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            _logger.LogDebug("Slack webhook URL not configured");
            return false;
        }

        if (articles == null || articles.Count == 0)
        {
            _logger.LogDebug("No articles to send to Slack");
            return false;
        }

        try
        {
            _logger.LogDebug("Preparing to send {Count} articles to Slack webhook", articles.Count);

            var httpClient = _httpClientFactory.CreateClient();

            // Build messages and split if needed
            var messages = await BuildSlackMessagesAsync(articles);
            
            _logger.LogDebug("Sending {MessageCount} message(s) to Slack", messages.Count);

            // Send all messages
            int successCount = 0;
            foreach (var message in messages)
            {
                var payload = new
                {
                    text = message,
                    mrkdwn = true
                };

                var jsonContent = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(webhookUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    successCount++;
                }
                else
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Failed to send message to Slack webhook: {StatusCode} - {Error}", response.StatusCode, responseText);
                }
                
                // Add small delay between messages to avoid rate limiting
                await Task.Delay(200);
            }

            if (successCount > 0)
            {
                _logger.LogDebug("Successfully sent {SuccessCount}/{TotalCount} message(s) to Slack webhook", successCount, messages.Count);
                return true;
            }
            else
            {
                _logger.LogDebug("Failed to send any messages to Slack webhook");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error sending news to Slack webhook");
            return false;
        }
    }

    private async Task<List<string>> BuildSlackMessagesAsync(List<News> articles)
    {
        const int maxMessageLength = 30000;
        var messages = new List<string>();
        var currentMessage = new System.Text.StringBuilder();

        // Count total articles and verify top 5 per category
        var groupedByCategory = articles.GroupBy(a => a.Category).OrderBy(g => g.Key);
        int totalArticlesCount = 0;
        var articlesPerCategory = new Dictionary<string, int>();

        foreach (var categoryGroup in groupedByCategory)
        {
            var topArticles = categoryGroup.OrderByDescending(a => a.PublishedDate).Take(5).ToList();
            articlesPerCategory[categoryGroup.Key.ToString()] = topArticles.Count;
            totalArticlesCount += topArticles.Count;
        }

        // Log verification to console (for backend visibility)
        _logger.LogInformation("📊 SLACK SEND VERIFICATION - Total articles: {TotalCount} | Per category: {PerCategory}", 
            totalArticlesCount, 
            string.Join(", ", articlesPerCategory.Select(x => $"{x.Key}={x.Value}")));

        // Header with article count verification
        var header = $"📰 *Daily News Summary* 📰\n_Generated at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC_\n✅ *Total: {totalArticlesCount} articles ({string.Join(", ", articlesPerCategory.Select(x => $"{x.Key}: {x.Value}"))})_\n\n";
        currentMessage.Append(header);

        foreach (var categoryGroup in groupedByCategory)
        {
            var topArticles = categoryGroup.OrderByDescending(a => a.PublishedDate).Take(5).ToList();
            
            var categoryText = new System.Text.StringBuilder();
            categoryText.AppendLine($"*{categoryGroup.Key}* ({topArticles.Count} articles)");
            categoryText.AppendLine("═════════════════════════════════════════");

            int postNumber = 1;
            foreach (var article in topArticles)
            {
                var articleText = new System.Text.StringBuilder();
                articleText.AppendLine($"*{postNumber}. <{article.Url}|{article.Title}>*");
                articleText.AppendLine($"  _Source: {article.Source} | Published: {article.PublishedDate:yyyy-MM-dd HH:mm}Z_");
                
                // Original Summary
                if (!string.IsNullOrWhiteSpace(article.Summary))
                {
                    articleText.AppendLine();
                    articleText.AppendLine("📝 *SUMMARY:*");
                    articleText.AppendLine($"  {article.Summary}");
                }

                // Try to retrieve enhanced analysis for this article
                try
                {
                    var analysis = await _analysisResultRepository.GetByNewsIdAsync(article.Id);
                    if (analysis != null && !string.IsNullOrWhiteSpace(analysis.EnhancedAnalysis))
                    {
                        articleText.AppendLine();
                        articleText.AppendLine("💡 *ANALYSIS INSIGHTS:*");
                        
                        // Split analysis into lines and wrap text to 80 chars per line
                        var analysisLines = WrapText(analysis.EnhancedAnalysis, 80);
                        foreach (var line in analysisLines)
                        {
                            articleText.AppendLine($"  {line}");
                        }

                        // Add confidence level
                        var confidenceEmoji = analysis.ConfidenceLevel switch
                        {
                            "HIGH" => "✅",
                            "MEDIUM" => "⚠️",
                            "LOW" => "❌",
                            _ => "❓"
                        };
                        articleText.AppendLine();
                        articleText.AppendLine($"  *Confidence Level:* {confidenceEmoji} {analysis.ConfidenceLevel}");

                        // Add key sources
                        if (analysis.SourceUrls != null && analysis.SourceUrls.Length > 0)
                        {
                            articleText.AppendLine();
                            articleText.AppendLine("📚 *KEY SOURCES:*");
                            foreach (var sourceUrl in analysis.SourceUrls.Take(3)) // Limit to first 3 sources
                            {
                                if (!string.IsNullOrWhiteSpace(sourceUrl))
                                {
                                    articleText.AppendLine($"  • <{sourceUrl}|Source>");
                                }
                            }
                            if (analysis.SourceUrls.Length > 3)
                            {
                                articleText.AppendLine($"  ... and {analysis.SourceUrls.Length - 3} more sources");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error retrieving analysis for article {NewsId}", article.Id);
                    // Continue without analysis if there's an error
                }
                
                // Add separator between articles
                articleText.AppendLine();
                articleText.AppendLine("  ═════════════════════════════════════════");
                articleText.AppendLine();

                postNumber++;

                // Check if adding this article would exceed limit
                string potentialMessage = currentMessage.ToString() + categoryText.ToString() + articleText.ToString();
                
                if (potentialMessage.Length > maxMessageLength && currentMessage.Length > header.Length)
                {
                    // Save current message and start new one
                    messages.Add(currentMessage.ToString().TrimEnd());
                    currentMessage.Clear();
                    currentMessage.Append($"📰 *Daily News Summary (continued)* 📰\n\n");
                    currentMessage.Append(categoryText.ToString());
                }

                currentMessage.Append(articleText.ToString());
            }

            categoryText.AppendLine();
        }

        // Add remaining content
        if (currentMessage.Length > header.Length)
        {
            messages.Add(currentMessage.ToString().TrimEnd());
        }

        return messages;
    }

    /// <summary>
    /// Wraps text to specified character width, preserving words
    /// </summary>
    private List<string> WrapText(string text, int maxWidth)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();

        var lines = new List<string>();
        var words = text.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var currentLine = new System.Text.StringBuilder();

        foreach (var word in words)
        {
            if (currentLine.Length + word.Length + 1 > maxWidth && currentLine.Length > 0)
            {
                lines.Add(currentLine.ToString().Trim());
                currentLine.Clear();
            }

            if (currentLine.Length > 0)
                currentLine.Append(" ");
            
            currentLine.Append(word);
        }

        if (currentLine.Length > 0)
            lines.Add(currentLine.ToString().Trim());

        return lines;
    }
}
