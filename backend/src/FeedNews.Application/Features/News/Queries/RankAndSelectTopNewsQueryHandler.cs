using FeedNews.Application.Configuration;
using MediatR;
using Microsoft.Extensions.Options;
using NewsEntity = FeedNews.Domain.Entities.News;

namespace FeedNews.Application.Features.News.Queries;

public class RankAndSelectTopNewsQueryHandler : IRequestHandler<RankAndSelectTopNewsQuery, List<NewsEntity>>
{
    private readonly NewsConfiguration _newsConfig;

    public RankAndSelectTopNewsQueryHandler(IOptions<NewsConfiguration> newsConfig)
    {
        _newsConfig = newsConfig.Value;
    }

    public Task<List<NewsEntity>> Handle(RankAndSelectTopNewsQuery request, CancellationToken cancellationToken)
    {
        var topNews = request.AllNews
            .Where(n => n.Category == request.Category)
            .OrderByDescending(n => n.PublishedDate)
            .Take(_newsConfig.TopNewsPerCategory)
            .ToList();

        return Task.FromResult(topNews);
    }
}

