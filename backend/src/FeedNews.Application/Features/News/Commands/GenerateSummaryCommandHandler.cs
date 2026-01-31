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
            var summary = await _geminiService.SummarizeArticleAsync(request.News.Title, request.News.Url);
            request.News.Summary = summary;
            request.News.UpdatedAt = DateTime.UtcNow;
        }

        return request.News;
    }
}
