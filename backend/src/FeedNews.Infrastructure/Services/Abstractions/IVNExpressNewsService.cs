using FeedNews.Domain.Entities;
using FeedNews.Domain.Enums;

namespace FeedNews.Infrastructure.Services.Abstractions;

public interface IVNExpressNewsService
{
    Task<List<News>> FetchNewsByCategoryAsync(NewsCategory category);
}
