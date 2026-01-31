using FeedNews.Domain.Entities;
using FeedNews.Domain.Enums;

namespace FeedNews.Application.Contracts.Repositories;

public interface INewsRepository
{
    Task AddAsync(News news);
    
    Task UpdateAsync(News news);
    
    Task<List<News>> GetByCategoryAndRecentAsync(NewsCategory category, int topCount);
    
    Task<bool> ExistsByUrlAsync(string url);
}
