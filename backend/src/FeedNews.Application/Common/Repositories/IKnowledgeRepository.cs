using FeedNews.Domain.Entities;

namespace FeedNews.Application.Common.Repositories;

/// <summary>
/// Repository interface for KnowledgeEntry entities
/// </summary>
public interface IKnowledgeRepository
{
    /// <summary>
    /// Get all active knowledge entries for a specific category
    /// </summary>
    Task<List<KnowledgeEntry>> GetByCategoryAsync(string category);

    /// <summary>
    /// Search knowledge entries by keywords (searches in topic, description, and tags)
    /// </summary>
    Task<List<KnowledgeEntry>> SearchByKeywordsAsync(string keywords, string? category = null);

    /// <summary>
    /// Get all active knowledge entries
    /// </summary>
    Task<List<KnowledgeEntry>> GetActiveAsync();

    /// <summary>
    /// Get knowledge entry by ID
    /// </summary>
    Task<KnowledgeEntry?> GetByIdAsync(Guid id);

    /// <summary>
    /// Add a knowledge entry
    /// </summary>
    Task AddAsync(KnowledgeEntry entry);

    /// <summary>
    /// Update a knowledge entry
    /// </summary>
    Task UpdateAsync(KnowledgeEntry entry);

    /// <summary>
    /// Get all knowledge entries (including inactive)
    /// </summary>
    Task<List<KnowledgeEntry>> GetAllAsync();
}
