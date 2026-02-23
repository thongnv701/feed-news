using FeedNews.Application.Common.Repositories;
using FeedNews.Domain.Entities;
using FeedNews.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace FeedNews.Infrastructure.Repositories;

public class ArticleAnalysisResultRepository : IArticleAnalysisResultRepository
{
    private readonly FeedNewsContext _context;

    public ArticleAnalysisResultRepository(FeedNewsContext context)
    {
        _context = context;
    }

    public async Task<ArticleAnalysisResult?> GetByIdAsync(Guid id)
    {
        return await _context.ArticleAnalysisResults.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<ArticleAnalysisResult?> GetByNewsIdAsync(Guid newsId)
    {
        return await _context.ArticleAnalysisResults
            .FirstOrDefaultAsync(a => a.NewsId == newsId);
    }

    public async Task<List<ArticleAnalysisResult>> GetByCategoryAsync(string category)
    {
        return await _context.ArticleAnalysisResults
            .Where(a => a.Category == category)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ArticleAnalysisResult>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.ArticleAnalysisResults
            .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ArticleAnalysisResult>> GetAllAsync()
    {
        return await _context.ArticleAnalysisResults
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(ArticleAnalysisResult entity)
    {
        await _context.ArticleAnalysisResults.AddAsync(entity);
    }

    public async Task UpdateAsync(ArticleAnalysisResult entity)
    {
        _context.ArticleAnalysisResults.Update(entity);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.ArticleAnalysisResults.Remove(entity);
        }
    }
}
