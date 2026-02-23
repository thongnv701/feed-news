namespace FeedNews.Domain.Entities;

/// <summary>
/// Knowledge base entry for storing domain-specific knowledge.
/// Used to enhance article analysis with context and fact-checking.
/// </summary>
public class KnowledgeEntry
{
    /// <summary>
    /// Unique identifier for the knowledge entry
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Category of knowledge: Business, Technology, World, or Shared
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Topic/title of the knowledge entry (e.g., "AI Regulations", "Market Trends")
    /// </summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the knowledge (150-500 characters recommended)
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Source URL for reference and fact-checking
    /// </summary>
    public string? SourceUrl { get; set; }

    /// <summary>
    /// Confidence score from 0 to 1
    /// HIGH ≥ 0.8, MEDIUM 0.6-0.79, LOW < 0.6
    /// </summary>
    public decimal ConfidenceScore { get; set; }

    /// <summary>
    /// Computed confidence level based on score
    /// </summary>
    public string Confidence => ConfidenceScore >= 0.8m ? "HIGH"
                              : ConfidenceScore >= 0.6m ? "MEDIUM"
                              : "LOW";

    /// <summary>
    /// Hierarchical tags for categorization (e.g., "Economic/Markets/Inflation")
    /// </summary>
    public string[] Tags { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When the entry was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the entry was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
