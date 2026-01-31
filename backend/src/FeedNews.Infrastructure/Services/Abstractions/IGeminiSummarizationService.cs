namespace FeedNews.Infrastructure.Services.Abstractions;

public interface IGeminiSummarizationService
{
    Task<string> SummarizeArticleAsync(string title, string content);
}
