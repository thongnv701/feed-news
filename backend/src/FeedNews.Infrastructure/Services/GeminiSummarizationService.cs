using System.Text.Json;
using FeedNews.Application.Contracts.Services;
using FeedNews.Application.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FeedNews.Infrastructure.Services;

public class GeminiSummarizationService : IGeminiSummarizationService
{
    private readonly GeminiSettings _geminiSettings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GeminiSummarizationService> _logger;
    private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/{0}:generateContent";

    public GeminiSummarizationService(
        IOptions<GeminiSettings> geminiSettings,
        IHttpClientFactory httpClientFactory,
        ILogger<GeminiSummarizationService> logger)
    {
        _geminiSettings = geminiSettings.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string> SummarizeArticleAsync(string title, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            _logger.LogWarning("Attempting to summarize article with empty content: {Title}", title);
            return "Unable to generate summary - no content available.";
        }

        if (string.IsNullOrWhiteSpace(_geminiSettings.ApiKey))
        {
            _logger.LogWarning("Gemini API key not configured");
            return "Unable to generate summary - API key not configured.";
        }

        try
        {
            _logger.LogDebug("Starting summarization for article: {Title}", title);

            var prompt = $@"Please summarize the following news article in 200-500 words. Focus on key facts, implications, and main takeaways.

Article Title: {title}

Article Content:
{content}

Summary:";

            var httpClient = _httpClientFactory.CreateClient();
            
            var requestPayload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    maxOutputTokens = _geminiSettings.MaxTokens,
                    temperature = 0.7
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestPayload);
            var requestContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var url = $"{string.Format(GeminiApiUrl, _geminiSettings.Model)}?key={_geminiSettings.ApiKey}";
            var response = await httpClient.PostAsync(url, requestContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API error for article {Title}: {StatusCode} - {Error}", title, response.StatusCode, errorContent);
                return $"Error generating summary - API returned {response.StatusCode}";
            }

            var responseText = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<JsonElement>(responseText);

            var summary = ExtractSummaryFromResponse(responseJson);

            if (string.IsNullOrWhiteSpace(summary))
            {
                _logger.LogWarning("Received empty summary from Gemini for article: {Title}", title);
                return "Unable to generate summary - API returned empty response.";
            }

            _logger.LogDebug("Successfully generated summary for article: {Title}", title);
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating summary for article: {Title}", title);
            return $"Error generating summary: {ex.Message}";
        }
    }

    private string ExtractSummaryFromResponse(JsonElement responseJson)
    {
        try
        {
            if (responseJson.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                var firstCandidate = candidates[0];
                if (firstCandidate.TryGetProperty("content", out var contentObj))
                {
                    if (contentObj.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                    {
                        var firstPart = parts[0];
                        if (firstPart.TryGetProperty("text", out var text))
                        {
                            return text.GetString() ?? string.Empty;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting summary from Gemini response");
        }

        return string.Empty;
    }
}

