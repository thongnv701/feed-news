using FeedNews.Domain.Entities;

namespace FeedNews.Application.Common.Repositories;

/// <summary>
/// Repository interface for KnowledgeDispute entities
/// </summary>
public interface IKnowledgeDisputeRepository
{
    /// <summary>
    /// Add a dispute record
    /// </summary>
    Task AddAsync(KnowledgeDispute dispute);

    /// <summary>
    /// Get dispute by ID
    /// </summary>
    Task<KnowledgeDispute?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get all disputes for a knowledge entry
    /// </summary>
    Task<List<KnowledgeDispute>> GetByKnowledgeEntryAsync(Guid knowledgeEntryId);

    /// <summary>
    /// Get all active (unresolved) disputes
    /// </summary>
    Task<List<KnowledgeDispute>> GetActiveDisputesAsync();
}
