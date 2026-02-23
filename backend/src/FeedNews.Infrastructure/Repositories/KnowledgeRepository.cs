using FeedNews.Application.Common.Repositories;
using FeedNews.Domain.Entities;
using FeedNews.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace FeedNews.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for KnowledgeEntry entities
/// </summary>
public class KnowledgeRepository : IKnowledgeRepository
{
    private readonly FeedNewsContext _context;

    public KnowledgeRepository(FeedNewsContext context)
    {
        _context = context;
    }

    public async Task<List<KnowledgeEntry>> GetByCategoryAsync(string category)
    {
        return await _context.KnowledgeEntries
            .Where(k => k.Category == category && k.IsActive)
            .OrderByDescending(k => k.ConfidenceScore)
            .ThenByDescending(k => k.UpdatedAt)
            .ToListAsync();
    }

    public async Task<List<KnowledgeEntry>> SearchByKeywordsAsync(string keywords, string? category = null)
    {
        var keywordArray = keywords.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(k => k.ToLower())
            .ToList();

        var query = _context.KnowledgeEntries.Where(k => k.IsActive);

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(k => k.Category == category);
        }

        var results = await query.ToListAsync();

        // Filter by keywords in topic, description, and tags
        var filteredResults = results
            .Where(k =>
            {
                var topic = k.Topic.ToLower();
                var description = k.Description.ToLower();
                var tags = string.Join(" ", k.Tags).ToLower();
                var combined = $"{topic} {description} {tags}";

                return keywordArray.Any(keyword => combined.Contains(keyword));
            })
            .OrderByDescending(k => k.ConfidenceScore)
            .ThenByDescending(k => k.UpdatedAt)
            .ToList();

        return filteredResults;
    }

    public async Task<List<KnowledgeEntry>> GetActiveAsync()
    {
        return await _context.KnowledgeEntries
            .Where(k => k.IsActive)
            .OrderByDescending(k => k.UpdatedAt)
            .ToListAsync();
    }

    public async Task<KnowledgeEntry?> GetByIdAsync(Guid id)
    {
        return await _context.KnowledgeEntries.FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task AddAsync(KnowledgeEntry entry)
    {
        entry.CreatedAt = DateTime.UtcNow;
        entry.UpdatedAt = DateTime.UtcNow;
        await _context.KnowledgeEntries.AddAsync(entry);
    }

    public async Task UpdateAsync(KnowledgeEntry entry)
    {
        entry.UpdatedAt = DateTime.UtcNow;
        _context.KnowledgeEntries.Update(entry);
        await Task.CompletedTask;
    }

    public async Task<List<KnowledgeEntry>> GetAllAsync()
    {
        return await _context.KnowledgeEntries
            .OrderByDescending(k => k.UpdatedAt)
            .ToListAsync();
    }
}
