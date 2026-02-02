using FeedNews.Application.Contracts.Services;
using MediatR;
using NewsEntity = FeedNews.Domain.Entities.News;

namespace FeedNews.Application.Features.News.Commands;

/// <summary>
/// Handles fetching article content from news URLs
/// </summary>
public class FetchArticleContentCommandHandler : IRequestHandler<FetchArticleContentCommand, NewsEntity>
{
    private readonly IArticleContentFetchService _contentFetchService;

    public FetchArticleContentCommandHandler(IArticleContentFetchService contentFetchService)
    {
        _contentFetchService = contentFetchService;
    }

    public async Task<NewsEntity> Handle(FetchArticleContentCommand request, CancellationToken cancellationToken)
    {
        // Only fetch if content is not already populated
        if (string.IsNullOrWhiteSpace(request.News.Content))
        {
            var content = await _contentFetchService.FetchArticleContentAsync(request.News.Url, request.News.Title);
            request.News.Content = content;
            request.News.UpdatedAt = DateTime.UtcNow;
        }

        return request.News;
    }
}
