using FeedNews.Application.Contracts.Services;
using FeedNews.Application.Features.Knowledge.Commands;
using FeedNews.Application.Features.Knowledge.Queries;
using MediatR;
using Microsoft.Extensions.Logging;
using NewsEntity = FeedNews.Domain.Entities.News;

namespace FeedNews.Application.Features.News.Commands;

public class GenerateSummaryCommandHandler : IRequestHandler<GenerateSummaryCommand, NewsEntity>
{
    private readonly IGeminiSummarizationService _geminiService;
    private readonly IMediator _mediator;
    private readonly IAnalysisEnhancementService _analysisEnhancementService;
    private readonly ILogger<GenerateSummaryCommandHandler> _logger;

    public GenerateSummaryCommandHandler(
        IGeminiSummarizationService geminiService,
        IMediator mediator,
        IAnalysisEnhancementService analysisEnhancementService,
        ILogger<GenerateSummaryCommandHandler> logger)
    {
        _geminiService = geminiService;
        _mediator = mediator;
        _analysisEnhancementService = analysisEnhancementService;
        _logger = logger;
    }

    public async Task<NewsEntity> Handle(GenerateSummaryCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.News.Summary))
        {
            // Use content if available, otherwise fallback to title
            var textToSummarize = !string.IsNullOrWhiteSpace(request.News.Content) 
                ? request.News.Content 
                : request.News.Title;
                
            var summary = await _geminiService.SummarizeArticleAsync(request.News.Title, textToSummarize);
            request.News.Summary = summary;
            request.News.UpdatedAt = DateTime.UtcNow;
        }

        // Generate enhanced analysis - always attempt if category is not default
        try
        {
            var categoryString = request.News.Category.ToString();
            if (!string.IsNullOrWhiteSpace(categoryString) && categoryString != "0")
            {
                _logger.LogInformation("Generating enhanced analysis for News: {NewsId}, Category: {Category}",
                    request.News.Id, categoryString);

                // Step 1: Get relevant knowledge based on summary and category
                var relevantKnowledge = await _mediator.Send(
                    new GetRelevantKnowledgeQuery(categoryString, request.News.Summary),
                    cancellationToken);

                if (relevantKnowledge.Any())
                {
                    _logger.LogInformation("Retrieved {Count} relevant knowledge entries", relevantKnowledge.Count);

                    // Step 2: Get analysis questions for category
                    var questions = await _analysisEnhancementService.GetQuestionsForCategory(categoryString);

                    // Step 3: Generate enhanced analysis
                    var enhancedAnalysisCommand = new GenerateEnhancedAnalysisCommand(
                        request.News.Id,
                        categoryString,
                        request.News.Summary,
                        relevantKnowledge,
                        questions);

                    var analysisResult = await _mediator.Send(enhancedAnalysisCommand, cancellationToken);

                    // Step 4: Save analysis result to database
                    var saveSuccess = await _mediator.Send(
                        new SaveAnalysisResultCommand(analysisResult),
                        cancellationToken);

                    if (saveSuccess)
                    {
                        _logger.LogInformation("Successfully saved enhanced analysis for News: {NewsId}", request.News.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to save enhanced analysis for News: {NewsId}", request.News.Id);
                    }
                }
                else
                {
                    _logger.LogInformation("No relevant knowledge found for category: {Category}", categoryString);
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the entire pipeline - enhanced analysis is optional
            _logger.LogError(ex, "Error generating enhanced analysis for News: {NewsId}", request.News.Id);
        }

        return request.News;
    }
}
