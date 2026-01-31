using FeedNews.Application.Contracts.Repositories;
using FeedNews.Domain.Entities;
using FeedNews.Domain.Enums;
using FeedNews.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace FeedNews.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for managing News entity persistence operations.
/// Handles CRUD operations and specialized queries for news articles.
/// </summary>
public class NewsRepository : INewsRepository
{
    private readonly FeedNewsContext _context;

    public NewsRepository(FeedNewsContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Adds a new news article to the database.
    /// </summary>
    /// <param name="news">The news entity to add</param>
    public async Task AddAsync(News news)
    {
        await _context.NewsFeeds.AddAsync(news).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates an existing news article in the database.
    /// </summary>
    /// <param name="news">The news entity to update</param>
    public async Task UpdateAsync(News news)
    {
        _context.NewsFeeds.Update(news);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves the most recent news articles for a specific category.
    /// </summary>
    /// <param name="category">The news category to filter by</param>
    /// <param name="topCount">Number of top articles to retrieve (default: 5)</param>
    /// <returns>List of news articles sorted by published date descending</returns>
    public async Task<List<News>> GetByCategoryAndRecentAsync(NewsCategory category, int topCount = 5)
    {
        return await _context.NewsFeeds
            .Where(n => n.Category == category)
            .OrderByDescending(n => n.PublishedDate)
            .Take(topCount)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Checks if a news article with the given URL already exists in the database.
    /// Used to prevent duplicate articles from being stored.
    /// </summary>
    /// <param name="url">The article URL to check</param>
    /// <returns>True if article exists, false otherwise</returns>
    public async Task<bool> ExistsByUrlAsync(string url)
    {
        return await _context.NewsFeeds
            .AnyAsync(n => n.Url == url)
            .ConfigureAwait(false);
    }
}
