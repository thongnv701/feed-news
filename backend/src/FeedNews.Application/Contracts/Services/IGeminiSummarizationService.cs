namespace FeedNews.Application.Contracts.Services;

public interface IGeminiSummarizationService
{
    Task<string> SummarizeArticleAsync(string title, string content);
}
