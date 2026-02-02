namespace FeedNews.Application.Contracts.Services;

/// <summary>
/// Service for extracting article content from URLs
/// </summary>
public interface IArticleContentFetchService
{
    /// <summary>
    /// Fetch the article body content from a given URL
    /// </summary>
    /// <param name="url">URL of the article</param>
    /// <param name="title">Article title (for logging)</param>
    /// <returns>Article content text, or empty string if unable to fetch</returns>
    Task<string> FetchArticleContentAsync(string url, string title);
}
