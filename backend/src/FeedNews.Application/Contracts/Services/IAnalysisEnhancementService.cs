using FeedNews.Domain.Entities;

namespace FeedNews.Application.Contracts.Services;

/// <summary>
/// Service for enhancing news article analysis with knowledge base context.
/// Extracts keywords from summaries, retrieves relevant knowledge, and questions.
/// </summary>
public interface IAnalysisEnhancementService
{
    /// <summary>
    /// Extract keywords from article summary using simple tokenization
    /// </summary>
    Task<List<string>> ExtractKeywords(string summary);

    /// <summary>
    /// Get knowledge base entries relevant to the article summary for a specific category
    /// </summary>
    Task<List<KnowledgeEntry>> GetRelevantKnowledge(string summary, string category);

    /// <summary>
    /// Get analysis questions for a specific category that should be answered in the analysis
    /// </summary>
    Task<List<AnalysisQuestion>> GetQuestionsForCategory(string category);

    /// <summary>
    /// Build the prompt for Gemini's second call with knowledge base context
    /// </summary>
    string BuildEnhancedPrompt(
        string originalSummary,
        List<KnowledgeEntry> relevantKnowledge,
        List<AnalysisQuestion> questions);
}
