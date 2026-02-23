using FeedNews.Domain.Entities;

namespace FeedNews.Application.Common.Repositories;

public interface IArticleAnalysisResultRepository
{
    Task<ArticleAnalysisResult?> GetByIdAsync(Guid id);
    Task<ArticleAnalysisResult?> GetByNewsIdAsync(Guid newsId);
    Task<List<ArticleAnalysisResult>> GetByCategoryAsync(string category);
    Task<List<ArticleAnalysisResult>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<List<ArticleAnalysisResult>> GetAllAsync();
    Task AddAsync(ArticleAnalysisResult entity);
    Task UpdateAsync(ArticleAnalysisResult entity);
    Task DeleteAsync(Guid id);
}
