using FeedNews.Application.Contracts.Services;
using FeedNews.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FeedNews.Application.Features.Knowledge.Queries;

public class GetRelevantKnowledgeQueryHandler : IRequestHandler<GetRelevantKnowledgeQuery, List<KnowledgeEntry>>
{
    private readonly IAnalysisEnhancementService _analysisEnhancementService;
    private readonly ILogger<GetRelevantKnowledgeQueryHandler> _logger;

    public GetRelevantKnowledgeQueryHandler(
        IAnalysisEnhancementService analysisEnhancementService,
        ILogger<GetRelevantKnowledgeQueryHandler> logger)
    {
        _analysisEnhancementService = analysisEnhancementService;
        _logger = logger;
    }

    public async Task<List<KnowledgeEntry>> Handle(GetRelevantKnowledgeQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving relevant knowledge for category: {Category}", request.Category);
        
        try
        {
            var relevantKnowledge = await _analysisEnhancementService.GetRelevantKnowledge(
                request.ArticleSummary,
                request.Category);

            _logger.LogInformation("Found {Count} relevant knowledge entries for category: {Category}", 
                relevantKnowledge.Count, request.Category);

            return relevantKnowledge;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving relevant knowledge for category: {Category}", request.Category);
            throw;
        }
    }
}
