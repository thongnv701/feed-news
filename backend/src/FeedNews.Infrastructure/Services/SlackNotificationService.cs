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
    private const string SlackApiUrl = "https://slack.com/api/chat.postMessage";

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
        if (string.IsNullOrWhiteSpace(_slackSettings.BotToken) || string.IsNullOrWhiteSpace(_slackSettings.ChannelId))
        {
            _logger.LogWarning("Slack credentials not configured - cannot send news");
            return false;
        }

        if (articles == null || articles.Count == 0)
        {
            _logger.LogWarning("No articles to send to Slack");
            return false;
        }

        try
        {
            _logger.LogInformation("Preparing to send {Count} articles to Slack", articles.Count);

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_slackSettings.BotToken}");

            var message = BuildSlackMessage(articles);
            
            var payload = new
            {
                channel = _slackSettings.ChannelId,
                text = message,
                mrkdwn = true
            };

            var jsonContent = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(SlackApiUrl, content);

            var responseText = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<JsonElement>(responseText);

            if (responseJson.TryGetProperty("ok", out var okProp) && okProp.GetBoolean())
            {
                _logger.LogInformation("Successfully sent {Count} articles to Slack", articles.Count);
                return true;
            }
            else
            {
                var errorMsg = responseJson.TryGetProperty("error", out var errorProp)
                    ? errorProp.GetString()
                    : "Unknown error";
                _logger.LogError("Failed to send message to Slack: {Error}", errorMsg);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending news to Slack");
            return false;
        }
    }

    private string BuildSlackMessage(List<News> articles)
    {
        var groupedByCategory = articles.GroupBy(a => a.Category).OrderBy(g => g.Key);
        
        var messageBuilder = new System.Text.StringBuilder();
        messageBuilder.AppendLine("📰 *Daily News Summary* 📰");
        messageBuilder.AppendLine($"_Generated at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC_");
        messageBuilder.AppendLine();

        foreach (var categoryGroup in groupedByCategory)
        {
            messageBuilder.AppendLine($"*{categoryGroup.Key}*");
            messageBuilder.AppendLine("---");

            foreach (var article in categoryGroup.OrderByDescending(a => a.PublishedDate).Take(5))
            {
                messageBuilder.AppendLine($"• <{article.Url}|{article.Title}>");
                messageBuilder.AppendLine($"  _Source: {article.Source} | Published: {article.PublishedDate:yyyy-MM-dd HH:mm}Z_");
                
                if (!string.IsNullOrWhiteSpace(article.Summary))
                {
                    var truncatedSummary = article.Summary.Length > 300 
                        ? article.Summary[..297] + "..." 
                        : article.Summary;
                    messageBuilder.AppendLine($"  {truncatedSummary}");
                }
                
                messageBuilder.AppendLine();
            }

            messageBuilder.AppendLine();
        }

        return messageBuilder.ToString();
    }
}
