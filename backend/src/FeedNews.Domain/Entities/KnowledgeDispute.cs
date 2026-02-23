namespace FeedNews.Domain.Entities;

/// <summary>
/// Tracks contradictory or conflicting knowledge entries in the knowledge base.
/// Used to identify and manage disputes for later resolution.
/// Phase 2 feature for advanced knowledge management.
/// </summary>
public class KnowledgeDispute
{
    /// <summary>
    /// Unique identifier for the dispute record
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the main knowledge entry
    /// </summary>
    public Guid KnowledgeEntryId { get; set; }

    /// <summary>
    /// Foreign key to the conflicting knowledge entry
    /// </summary>
    public Guid ConflictingEntryId { get; set; }

    /// <summary>
    /// Description of why these entries conflict
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// When the dispute was identified
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the dispute was resolved (null if not yet resolved)
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// How the dispute was resolved or notes on resolution approach
    /// </summary>
    public string? Resolution { get; set; }
}
