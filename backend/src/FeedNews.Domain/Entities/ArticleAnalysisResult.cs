namespace FeedNews.Domain.Entities;

/// <summary>
/// Stores the results of enhanced AI analysis for an article.
/// Contains original summary, enhanced analysis with knowledge base context,
/// referenced sources, and confidence levels.
/// </summary>
public class ArticleAnalysisResult
{
    /// <summary>
    /// Unique identifier for the analysis result
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the News article being analyzed
    /// </summary>
    public Guid NewsId { get; set; }

    /// <summary>
    /// Category of the article: Business, Technology, or World
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Original summary from first Gemini call (400-800 words)
    /// </summary>
    public string OriginalSummary { get; set; } = string.Empty;

    /// <summary>
    /// Enhanced analysis from second Gemini call with knowledge base context
    /// Includes analysis insights, key findings, and questions answered
    /// </summary>
    public string? EnhancedAnalysis { get; set; }

    /// <summary>
    /// Topics from knowledge base that were referenced in analysis
    /// </summary>
    public string[] ReferencedKnowledge { get; set; } = Array.Empty<string>();

    /// <summary>
    /// URLs of sources cited in the enhanced analysis
    /// Used for fact-checking and reference verification
    /// </summary>
    public string[] SourceUrls { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Overall confidence level of the analysis: HIGH, MEDIUM, or LOW
    /// </summary>
    public string? ConfidenceLevel { get; set; }

    /// <summary>
    /// JSON string containing { "question": "answer" } pairs
    /// Questions from analysis questions answered for this article
    /// </summary>
    public string? QuestionsAnswered { get; set; }

    /// <summary>
    /// When the analysis was created/completed
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
