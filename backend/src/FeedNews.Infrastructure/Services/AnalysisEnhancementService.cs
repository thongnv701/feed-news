using System.Text;
using System.Text.RegularExpressions;
using FeedNews.Application.Common.Repositories;
using FeedNews.Application.Contracts.Services;
using FeedNews.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FeedNews.Infrastructure.Services;

/// <summary>
/// Implementation of IAnalysisEnhancementService.
/// Provides functionality to extract keywords, retrieve relevant knowledge,
/// and build enhanced prompts for AI analysis.
/// </summary>
public class AnalysisEnhancementService : IAnalysisEnhancementService
{
    private readonly IKnowledgeBaseService _knowledgeBaseService;
    private readonly IAnalysisQuestionRepository _questionRepository;
    private readonly ILogger<AnalysisEnhancementService> _logger;

    // Common stopwords to exclude from keyword extraction
    private static readonly HashSet<string> Stopwords = new()
    {
        "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for",
        "of", "with", "by", "from", "as", "is", "was", "are", "be", "been",
        "being", "have", "has", "had", "do", "does", "did", "will", "would",
        "could", "should", "may", "might", "must", "can", "this", "that",
        "these", "those", "i", "you", "he", "she", "it", "we", "they", "what",
        "which", "who", "when", "where", "why", "how", "all", "each", "every",
        "both", "few", "more", "most", "other", "some", "such", "no", "nor",
        "not", "only", "same", "so", "than", "too", "very", "just"
    };

    public AnalysisEnhancementService(
        IKnowledgeBaseService knowledgeBaseService,
        IAnalysisQuestionRepository questionRepository,
        ILogger<AnalysisEnhancementService> logger)
    {
        _knowledgeBaseService = knowledgeBaseService;
        _questionRepository = questionRepository;
        _logger = logger;
    }

    public async Task<List<string>> ExtractKeywords(string summary)
    {
        try
        {
            _logger.LogDebug("Extracting keywords from summary (length: {Length})", summary?.Length ?? 0);

            if (string.IsNullOrWhiteSpace(summary))
            {
                return new List<string>();
            }

            // Convert to lowercase and remove punctuation
            var cleanedText = Regex.Replace(summary.ToLower(), @"[^\w\s]", " ");

            // Split into words
            var words = cleanedText.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 3) // Exclude short words
                .Where(w => !Stopwords.Contains(w))
                .Distinct()
                .Take(20) // Limit to top 20 keywords
                .ToList();

            _logger.LogDebug("Extracted {Count} keywords from summary", words.Count);
            return words;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting keywords from summary");
            return new List<string>();
        }
    }

    public async Task<List<KnowledgeEntry>> GetRelevantKnowledge(string summary, string category)
    {
        try
        {
            _logger.LogDebug("Retrieving relevant knowledge for category: {Category}", category);

            // First get all knowledge entries for the category
            var categoryKnowledge = await _knowledgeBaseService.GetByCategory(category);

            if (categoryKnowledge.Count == 0)
            {
                _logger.LogDebug("No knowledge entries found for category: {Category}", category);
                return new List<KnowledgeEntry>();
            }

            // Extract keywords from summary
            var keywords = await ExtractKeywords(summary);

            if (keywords.Count == 0)
            {
                // If no keywords extracted, return all category knowledge
                return categoryKnowledge.Take(5).ToList();
            }

            // Score each knowledge entry based on keyword matches
            var scoredKnowledge = categoryKnowledge
                .Select(k =>
                {
                    var score = CalculateRelevanceScore(k, keywords);
                    return new { Knowledge = k, Score = score };
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Knowledge.ConfidenceScore)
                .Take(5)
                .Select(x => x.Knowledge)
                .ToList();

            _logger.LogDebug("Retrieved {Count} relevant knowledge entries for category: {Category}", 
                scoredKnowledge.Count, category);

            return scoredKnowledge;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving relevant knowledge for category: {Category}", category);
            return new List<KnowledgeEntry>();
        }
    }

    public async Task<List<AnalysisQuestion>> GetQuestionsForCategory(string category)
    {
        try
        {
            _logger.LogDebug("Retrieving analysis questions for category: {Category}", category);

            var questions = await _questionRepository.GetByCategoryAsync(category);

            _logger.LogDebug("Retrieved {Count} analysis questions for category: {Category}", 
                questions.Count, category);

            return questions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analysis questions for category: {Category}", category);
            return new List<AnalysisQuestion>();
        }
    }

    public string BuildEnhancedPrompt(
        string originalSummary,
        List<KnowledgeEntry> relevantKnowledge,
        List<AnalysisQuestion> questions)
    {
        try
        {
            _logger.LogDebug("Building enhanced prompt with {KnowledgeCount} knowledge entries and {QuestionCount} questions",
                relevantKnowledge.Count, questions.Count);

            var prompt = new StringBuilder();

            prompt.AppendLine("You are a fact-checking analyst with access to an internal knowledge base.");
            prompt.AppendLine("Your role is to provide perspective-based analysis grounded in facts and sources.");
            prompt.AppendLine();

            prompt.AppendLine("ARTICLE SUMMARY:");
            prompt.AppendLine(originalSummary);
            prompt.AppendLine();

            if (relevantKnowledge.Any())
            {
                prompt.AppendLine("INTERNAL KNOWLEDGE BASE (References):");
                prompt.AppendLine("Below is our internal knowledge that may be relevant to this article:");
                prompt.AppendLine();

                foreach (var knowledge in relevantKnowledge)
                {
                    prompt.AppendLine($"• Topic: {knowledge.Topic}");
                    prompt.AppendLine($"  Description: {knowledge.Description}");
                    if (!string.IsNullOrEmpty(knowledge.SourceUrl))
                    {
                        prompt.AppendLine($"  Source: {knowledge.SourceUrl}");
                    }
                    prompt.AppendLine($"  Confidence: {knowledge.Confidence}");
                    prompt.AppendLine();
                }
            }

            if (questions.Any())
            {
                prompt.AppendLine("USER QUESTIONS TO ADDRESS:");
                prompt.AppendLine("Please address the following questions in your analysis:");
                prompt.AppendLine();

                foreach (var question in questions.OrderBy(q => q.Priority))
                {
                    prompt.AppendLine($"• {question.Question}");
                    if (!string.IsNullOrEmpty(question.Purpose))
                    {
                        prompt.AppendLine($"  (Context: {question.Purpose})");
                    }
                }
                prompt.AppendLine();
            }

            prompt.AppendLine("TASK:");
            prompt.AppendLine("1. Analyze the article against our knowledge base");
            prompt.AppendLine("2. Identify alignments with our knowledge");
            prompt.AppendLine("3. Highlight contradictions or new information");
            prompt.AppendLine("4. Answer each user question with specific references");
            prompt.AppendLine("5. Provide your perspective on:");
            prompt.AppendLine("   - How does this fit our understanding?");
            prompt.AppendLine("   - What's new or surprising?");
            prompt.AppendLine("   - What are the implications?");
            prompt.AppendLine();

            prompt.AppendLine("CRITICAL REQUIREMENTS:");
            prompt.AppendLine("- ALWAYS reference sources (URLs or knowledge base entries)");
            prompt.AppendLine("- NEVER make assumptions or fabricate citations");
            prompt.AppendLine("- Clearly mark confidence level: HIGH/MEDIUM/LOW");
            prompt.AppendLine("- Format: [Your analysis] | Sources: [URLs] | Confidence: HIGH/MEDIUM/LOW");
            prompt.AppendLine();

            prompt.AppendLine("OUTPUT FORMAT:");
            prompt.AppendLine("## Analysis Insights");
            prompt.AppendLine("[Your detailed analysis with references]");
            prompt.AppendLine();
            prompt.AppendLine("## Key Findings");
            prompt.AppendLine("- Finding 1: [supported by: source]");
            prompt.AppendLine("- Finding 2: [supported by: source]");
            prompt.AppendLine();

            if (questions.Any())
            {
                prompt.AppendLine("## Questions Addressed");
                foreach (var question in questions.OrderBy(q => q.Priority))
                {
                    prompt.AppendLine($"- Q: {question.Question}");
                    prompt.AppendLine("  A: [answer] (Confidence: HIGH/MEDIUM/LOW)");
                }
                prompt.AppendLine();
            }

            prompt.AppendLine("## Source References");
            prompt.AppendLine("[All URLs cited]");

            return prompt.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building enhanced prompt");
            throw;
        }
    }

    /// <summary>
    /// Calculate relevance score based on keyword matches in knowledge entry
    /// </summary>
    private decimal CalculateRelevanceScore(KnowledgeEntry knowledge, List<string> keywords)
    {
        decimal score = 0;

        var combinedText = $"{knowledge.Topic} {knowledge.Description} {string.Join(" ", knowledge.Tags)}"
            .ToLower();

        foreach (var keyword in keywords)
        {
            if (combinedText.Contains(keyword))
            {
                // Weight: topic matches get 3 points, description/tags get 1 point
                if (knowledge.Topic.ToLower().Contains(keyword))
                {
                    score += 3;
                }
                else
                {
                    score += 1;
                }
            }
        }

        return score;
    }
}
