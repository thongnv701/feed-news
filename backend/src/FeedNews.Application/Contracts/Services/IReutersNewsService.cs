using FeedNews.Domain.Entities;
using FeedNews.Domain.Enums;

namespace FeedNews.Application.Contracts.Services;

public interface IReutersNewsService
{
    Task<List<News>> FetchNewsByCategoryAsync(NewsCategory category);
}
