using System.Text.Json;
using FeedNews.Application.Contracts.Services;
using FeedNews.Application.Configuration;
using FeedNews.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FeedNews.Infrastructure.Services;

/// <summary>
/// Implementation of IGeminiEnhancementService.
/// Calls Gemini API with enhanced context (knowledge base + questions) for detailed analysis.
/// </summary>
public class GeminiEnhancementService : IGeminiEnhancementService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GeminiSettings _geminiSettings;
    private readonly ILogger<GeminiEnhancementService> _logger;
    private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/{0}:generateContent";

    public GeminiEnhancementService(
        IHttpClientFactory httpClientFactory,
        IOptions<GeminiSettings> geminiSettings,
        ILogger<GeminiEnhancementService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _geminiSettings = geminiSettings.Value;
        _logger = logger;
    }

    public async Task<string> GenerateEnhancedAnalysis(
        string originalSummary,
        List<KnowledgeEntry> relevantKnowledge,
        List<AnalysisQuestion> questions)
    {
        if (string.IsNullOrWhiteSpace(originalSummary))
        {
            _logger.LogWarning("Attempting to enhance analysis with empty summary");
            return "Unable to generate enhanced analysis - no summary provided.";
        }

        if (string.IsNullOrWhiteSpace(_geminiSettings.ApiKey))
        {
            _logger.LogWarning("Gemini API key not configured");
            return "Unable to generate enhanced analysis - API key not configured.";
        }

        try
        {
            _logger.LogDebug("Starting enhanced analysis generation with {KnowledgeCount} knowledge entries and {QuestionCount} questions",
                relevantKnowledge.Count, questions.Count);

            // Build the enhanced prompt (from AnalysisEnhancementService)
            // For now we'll use a simpler approach - just include the context in the request
            var prompt = BuildAnalysisPrompt(originalSummary, relevantKnowledge, questions);

            var httpClient = _httpClientFactory.CreateClient();

            // Increased maxOutputTokens to ensure full response without truncation
            var requestPayload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    maxOutputTokens = 2500, // Increased for detailed analysis
                    temperature = 0.7
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestPayload);
            var requestContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var url = $"{string.Format(GeminiApiUrl, _geminiSettings.Model)}?key={_geminiSettings.ApiKey}";
            var response = await httpClient.PostAsync(url, requestContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Gemini API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return $"Error generating enhanced analysis - API returned {response.StatusCode}";
            }

            var responseText = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<JsonElement>(responseText);

            var analysis = ExtractAnalysisFromResponse(responseJson);

            if (string.IsNullOrWhiteSpace(analysis))
            {
                _logger.LogWarning("Received empty enhanced analysis from Gemini");
                return "Unable to generate enhanced analysis - API returned empty response.";
            }

            _logger.LogDebug("Successfully generated enhanced analysis");
            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating enhanced analysis");
            return $"Error generating enhanced analysis: {ex.Message}";
        }
    }

    /// <summary>
    /// Extract the analysis text from Gemini API response
    /// </summary>
    private string ExtractAnalysisFromResponse(JsonElement responseJson)
    {
        try
        {
            if (responseJson.TryGetProperty("candidates", out var candidates) &&
                candidates.GetArrayLength() > 0)
            {
                var firstCandidate = candidates[0];
                if (firstCandidate.TryGetProperty("content", out var content) &&
                    content.TryGetProperty("parts", out var parts) &&
                    parts.GetArrayLength() > 0)
                {
                    var firstPart = parts[0];
                    if (firstPart.TryGetProperty("text", out var textElement))
                    {
                        return textElement.GetString() ?? string.Empty;
                    }
                }
            }

            _logger.LogWarning("Could not extract analysis from Gemini response");
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting analysis from Gemini response");
            return string.Empty;
        }
    }

    /// <summary>
    /// Build the analysis prompt with context
    /// </summary>
    private string BuildAnalysisPrompt(
        string originalSummary,
        List<KnowledgeEntry> relevantKnowledge,
        List<AnalysisQuestion> questions)
    {
        var prompt = $@"You are a fact-checking analyst with access to an internal knowledge base.
Your role is to provide perspective-based analysis grounded in facts and sources.

ARTICLE SUMMARY:
{originalSummary}

";

        if (relevantKnowledge.Any())
        {
            prompt += "INTERNAL KNOWLEDGE BASE (References):\n";
            prompt += "Below is our internal knowledge that may be relevant to this article:\n\n";
            foreach (var knowledge in relevantKnowledge)
            {
                prompt += $"• Topic: {knowledge.Topic}\n";
                prompt += $"  Description: {knowledge.Description}\n";
                if (!string.IsNullOrEmpty(knowledge.SourceUrl))
                {
                    prompt += $"  Source: {knowledge.SourceUrl}\n";
                }
                prompt += $"  Confidence: {knowledge.Confidence}\n\n";
            }
            prompt += "\n";
        }

        if (questions.Any())
        {
            prompt += "USER QUESTIONS TO ADDRESS:\n";
            prompt += "Please address the following questions in your analysis:\n\n";
            foreach (var question in questions.OrderBy(q => q.Priority))
            {
                prompt += $"• {question.Question}\n";
                if (!string.IsNullOrEmpty(question.Purpose))
                {
                    prompt += $"  (Context: {question.Purpose})\n";
                }
            }
            prompt += "\n";
        }

        prompt += @"TASK:
1. Analyze the article against our knowledge base
2. Identify alignments with our knowledge
3. Highlight contradictions or new information
4. Answer each user question with specific references
5. Provide your perspective on:
   - How does this fit our understanding?
   - What's new or surprising?
   - What are the implications?

CRITICAL REQUIREMENTS:
- ALWAYS reference sources (URLs or knowledge base entries)
- NEVER make assumptions or fabricate citations
- Clearly mark confidence level: HIGH/MEDIUM/LOW
- Format: [Your analysis] | Sources: [URLs] | Confidence: HIGH/MEDIUM/LOW

OUTPUT FORMAT:
## Analysis Insights
[Your detailed analysis with references]

## Key Findings
- Finding 1: [supported by: source]
- Finding 2: [supported by: source]

";

        if (questions.Any())
        {
            prompt += "## Questions Addressed\n";
            foreach (var question in questions.OrderBy(q => q.Priority))
            {
                prompt += $"- Q: {question.Question}\n";
                prompt += "  A: [answer] (Confidence: HIGH/MEDIUM/LOW)\n";
            }
            prompt += "\n";
        }

        prompt += "## Source References\n[All URLs cited]";

        return prompt;
    }
}
