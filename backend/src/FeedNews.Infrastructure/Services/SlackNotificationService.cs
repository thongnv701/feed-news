using System.Text.Json;
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
    private readonly ILogger<SlackNotificationService> _logger;

    public SlackNotificationService(
        IOptions<SlackSettings> slackSettings,
        IHttpClientFactory httpClientFactory,
        ILogger<SlackNotificationService> logger)
    {
        _slackSettings = slackSettings.Value;
        _httpClientFactory = httpClientFactory;
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
            var messages = BuildSlackMessages(articles);
            
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

    private List<string> BuildSlackMessages(List<News> articles)
    {
        const int maxMessageLength = 13000; // Safe limit for webhook messages
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
            categoryText.AppendLine("---");

            int postNumber = 1;
            foreach (var article in topArticles)
            {
                var articleText = new System.Text.StringBuilder();
                articleText.AppendLine($"*{postNumber}. <{article.Url}|{article.Title}>*");
                articleText.AppendLine($"  _Source: {article.Source} | Published: {article.PublishedDate:yyyy-MM-dd HH:mm}Z_");
                
                if (!string.IsNullOrWhiteSpace(article.Summary))
                {
                    // Keep full summary, don't truncate - let Slack handle it
                    articleText.AppendLine($"  {article.Summary}");
                }
                
                // Add separator between articles
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
}
