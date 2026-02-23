using FeedNews.Domain.Entities;
using FeedNews.Domain.Enums;

namespace FeedNews.Application.Contracts.Repositories;

public interface INewsRepository
{
    Task AddAsync(News news);
    
    Task UpdateAsync(News news);
    
    Task<List<News>> GetByCategoryAndRecentAsync(NewsCategory category, int topCount);
    
    Task<bool> ExistsByUrlAsync(string url);
    
    /// <summary>
    /// Checks if a news article with the given ID exists in the database.
    /// </summary>
    /// <param name="id">The news ID to check</param>
    /// <returns>True if news exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id);
}
