using FeedNews.Application.Contracts.Services;
using MediatR;
using NewsEntity = FeedNews.Domain.Entities.News;

namespace FeedNews.Application.Features.News.Commands;

public class GenerateSummaryCommandHandler : IRequestHandler<GenerateSummaryCommand, NewsEntity>
{
    private readonly IGeminiSummarizationService _geminiService;

    public GenerateSummaryCommandHandler(IGeminiSummarizationService geminiService)
    {
        _geminiService = geminiService;
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

        return request.News;
    }
}
