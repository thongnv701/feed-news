using FeedNews.Domain.Entities;

namespace FeedNews.Application.Contracts.Services;

/// <summary>
/// Service for calling Gemini AI with enhanced context (knowledge base + questions).
/// Generates detailed analysis based on article summary, internal knowledge, and specific questions.
/// </summary>
public interface IGeminiEnhancementService
{
    /// <summary>
    /// Generate enhanced analysis by calling Gemini with knowledge base context
    /// </summary>
    Task<string> GenerateEnhancedAnalysis(
        string originalSummary,
        List<KnowledgeEntry> relevantKnowledge,
        List<AnalysisQuestion> questions);
}
