namespace FeedNews.Domain.Entities;

/// <summary>
/// Analysis questions that are used to guide Gemini's enhanced analysis per category.
/// Each question helps AI focus on specific concerns relevant to the category.
/// </summary>
public class AnalysisQuestion
{
    /// <summary>
    /// Unique identifier for the question
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Category this question applies to: Business, Technology, World
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// The question itself (e.g., "How does this affect market stability?")
    /// </summary>
    public string Question { get; set; } = string.Empty;

    /// <summary>
    /// Purpose/context for why this question matters
    /// </summary>
    public string? Purpose { get; set; }

    /// <summary>
    /// Priority level: 1=high, 2=medium, 3=low
    /// Used to order questions in analysis
    /// </summary>
    public int Priority { get; set; } = 2;

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When the question was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the question was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
