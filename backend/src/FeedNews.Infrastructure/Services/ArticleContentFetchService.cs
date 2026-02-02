using FeedNews.Application.Contracts.Services;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace FeedNews.Infrastructure.Services;

/// <summary>
/// Service for fetching and extracting article content from news URLs
/// </summary>
public class ArticleContentFetchService : IArticleContentFetchService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ArticleContentFetchService> _logger;
    
    // Common content selectors for different news sites
    private static readonly string[] CommonContentSelectors = new[]
    {
        "article",
        "[role='main']",
        ".article-content",
        ".post-content",
        ".entry-content",
        ".story-body",
        ".article-body",
        "main",
        ".content",
        "#content"
    };

    public ArticleContentFetchService(
        IHttpClientFactory httpClientFactory,
        ILogger<ArticleContentFetchService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string> FetchArticleContentAsync(string url, string title)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            _logger.LogWarning("Attempting to fetch content with empty URL for article: {Title}", title);
            return string.Empty;
        }

        try
        {
            _logger.LogDebug("Fetching article content from URL: {Url}", url);
            
            var httpClient = _httpClientFactory.CreateClient();
            
            // Set a browser-like user agent to avoid being blocked
            httpClient.DefaultRequestHeaders.Add("User-Agent", 
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            
            // Set a 10 second timeout for fetching each article
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            using var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();
            
            // Extract text content from HTML
            var content = ExtractTextFromHtml(html);

            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("Extracted empty content from article: {Title} at {Url}", title, url);
                return string.Empty;
            }

            _logger.LogDebug("Successfully extracted {CharCount} characters from article: {Title}", 
                content.Length, title);
            
            return content;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Timeout fetching article content from {Url} for article {Title} (exceeded 10s)", url, title);
            return string.Empty;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to fetch article content from {Url} for article {Title}", url, title);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting article content from {Url} for article {Title}", url, title);
            return string.Empty;
        }
    }

    /// <summary>
    /// Extract clean text content from HTML
    /// </summary>
    private string ExtractTextFromHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        try
        {
            // Remove script and style tags
            html = Regex.Replace(html, @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>", " ", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"<style\b[^<]*(?:(?!<\/style>)<[^<]*)*<\/style>", " ", RegexOptions.IgnoreCase);

            // Remove HTML tags but keep text
            html = Regex.Replace(html, @"<[^>]+>", " ");

            // Decode HTML entities
            html = System.Net.WebUtility.HtmlDecode(html);

            // Clean up whitespace
            html = Regex.Replace(html, @"\s+", " ");
            html = html.Trim();

            return html;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error cleaning HTML content");
            return string.Empty;
        }
    }
}
