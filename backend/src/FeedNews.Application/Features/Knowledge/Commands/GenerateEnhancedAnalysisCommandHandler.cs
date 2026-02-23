using FeedNews.Application.Common.Repositories;
using FeedNews.Application.Contracts.Services;
using FeedNews.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FeedNews.Application.Features.Knowledge.Commands;

public class GenerateEnhancedAnalysisCommandHandler : IRequestHandler<GenerateEnhancedAnalysisCommand, ArticleAnalysisResult>
{
    private readonly IAnalysisEnhancementService _analysisEnhancementService;
    private readonly IGeminiEnhancementService _geminiEnhancementService;
    private readonly ILogger<GenerateEnhancedAnalysisCommandHandler> _logger;

    public GenerateEnhancedAnalysisCommandHandler(
        IAnalysisEnhancementService analysisEnhancementService,
        IGeminiEnhancementService geminiEnhancementService,
        ILogger<GenerateEnhancedAnalysisCommandHandler> logger)
    {
        _analysisEnhancementService = analysisEnhancementService;
        _geminiEnhancementService = geminiEnhancementService;
        _logger = logger;
    }

    public async Task<ArticleAnalysisResult> Handle(GenerateEnhancedAnalysisCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating enhanced analysis for NewsId: {NewsId}, Category: {Category}", 
            request.NewsId, request.Category);

        try
        {
            // Build the enhanced prompt
            var enhancedPrompt = _analysisEnhancementService.BuildEnhancedPrompt(
                request.OriginalSummary,
                request.RelevantKnowledge,
                request.Questions);

            // Generate analysis using Gemini
            var enhancedAnalysis = await _geminiEnhancementService.GenerateEnhancedAnalysis(
                request.OriginalSummary,
                request.RelevantKnowledge,
                request.Questions);

            // Extract source URLs from knowledge entries
            var sourceUrls = request.RelevantKnowledge
                .Where(k => !string.IsNullOrEmpty(k.SourceUrl))
                .Select(k => k.SourceUrl!)
                .ToArray();

            // Extract referenced knowledge IDs
            var referencedKnowledge = request.RelevantKnowledge
                .Select(k => k.Id.ToString())
                .ToArray();

            // Determine confidence level based on knowledge quality and question count
            var averageConfidence = request.RelevantKnowledge.Any()
                ? request.RelevantKnowledge.Average(k => k.ConfidenceScore)
                : 0m;

            var confidenceLevel = averageConfidence >= 0.8m ? "HIGH"
                : averageConfidence >= 0.6m ? "MEDIUM"
                : "LOW";

            // Create analysis result
            var analysisResult = new ArticleAnalysisResult
            {
                Id = Guid.NewGuid(),
                NewsId = request.NewsId,
                Category = request.Category,
                OriginalSummary = request.OriginalSummary,
                EnhancedAnalysis = enhancedAnalysis,
                ReferencedKnowledge = referencedKnowledge,
                SourceUrls = sourceUrls,
                ConfidenceLevel = confidenceLevel,
                QuestionsAnswered = System.Text.Json.JsonSerializer.Serialize(
                    request.Questions.Select(q => new { q.Question, q.Purpose }).ToList()),
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Successfully generated enhanced analysis for NewsId: {NewsId}", request.NewsId);

            return analysisResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating enhanced analysis for NewsId: {NewsId}", request.NewsId);
            throw;
        }
    }
}
