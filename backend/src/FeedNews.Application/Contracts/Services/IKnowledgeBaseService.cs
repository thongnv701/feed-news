using FeedNews.Domain.Entities;

namespace FeedNews.Application.Contracts.Services;

/// <summary>
/// Service for managing and querying the knowledge base.
/// Provides methods to retrieve, add, update, and manage knowledge entries.
/// </summary>
public interface IKnowledgeBaseService
{
    /// <summary>
    /// Get all active knowledge entries for a specific category
    /// </summary>
    Task<List<KnowledgeEntry>> GetByCategory(string category);

    /// <summary>
    /// Search knowledge entries by keywords within optional category
    /// </summary>
    Task<List<KnowledgeEntry>> SearchByKeywords(string keywords, string? category = null);

    /// <summary>
    /// Get all active knowledge entries
    /// </summary>
    Task<List<KnowledgeEntry>> GetActive();

    /// <summary>
    /// Add a new knowledge entry
    /// </summary>
    Task<KnowledgeEntry> AddEntry(KnowledgeEntry entry);

    /// <summary>
    /// Update an existing knowledge entry
    /// </summary>
    Task<KnowledgeEntry> UpdateEntry(KnowledgeEntry entry);

    /// <summary>
    /// Soft delete a knowledge entry (marks as inactive)
    /// </summary>
    Task<bool> DeleteEntry(Guid id);

    /// <summary>
    /// Mark two knowledge entries as conflicting/disputed
    /// </summary>
    Task<bool> MarkAsDisputed(Guid entryId, Guid conflictingEntryId, string reason);

    /// <summary>
    /// Get a knowledge entry by ID
    /// </summary>
    Task<KnowledgeEntry?> GetById(Guid id);
}
