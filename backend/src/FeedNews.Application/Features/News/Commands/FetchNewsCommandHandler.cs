using FeedNews.Application.Common.Repositories;
using FeedNews.Application.Contracts.Repositories;
using FeedNews.Application.Contracts.Services;
using MediatR;
using NewsEntity = FeedNews.Domain.Entities.News;

namespace FeedNews.Application.Features.News.Commands;

public class FetchNewsCommandHandler : IRequestHandler<FetchNewsCommand, List<NewsEntity>>
{
    private readonly IReutersNewsService _reutersService;
    private readonly IVNExpressNewsService _vnExpressService;
    private readonly INewsRepository _newsRepository;

    public FetchNewsCommandHandler(
        IReutersNewsService reutersService,
        IVNExpressNewsService vnExpressService,
        INewsRepository newsRepository)
    {
        _reutersService = reutersService;
        _vnExpressService = vnExpressService;
        _newsRepository = newsRepository;
    }

    public async Task<List<NewsEntity>> Handle(FetchNewsCommand request, CancellationToken cancellationToken)
    {
        var reutersTasks = _reutersService.FetchNewsByCategoryAsync(request.Category);
        var vnExpressTasks = _vnExpressService.FetchNewsByCategoryAsync(request.Category);

        await Task.WhenAll(reutersTasks, vnExpressTasks);

        var allNews = new List<NewsEntity>();
        allNews.AddRange(await reutersTasks);
        allNews.AddRange(await vnExpressTasks);

        var uniqueNews = new List<NewsEntity>();
        foreach (var news in allNews)
        {
            var exists = await _newsRepository.ExistsByUrlAsync(news.Url);
            if (!exists)
            {
                uniqueNews.Add(news);
            }
        }

        return uniqueNews;
    }
}
