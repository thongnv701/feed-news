using FeedNews.Application.Contracts.Services;
using MediatR;
using NewsEntity = FeedNews.Domain.Entities.News;

namespace FeedNews.Application.Features.News.Commands;

public class FetchNewsCommandHandler : IRequestHandler<FetchNewsCommand, List<NewsEntity>>
{
    private readonly IReutersNewsService _reutersService;
    private readonly IVNExpressNewsService _vnExpressService;

    public FetchNewsCommandHandler(
        IReutersNewsService reutersService,
        IVNExpressNewsService vnExpressService)
    {
        _reutersService = reutersService;
        _vnExpressService = vnExpressService;
    }

    public async Task<List<NewsEntity>> Handle(FetchNewsCommand request, CancellationToken cancellationToken)
    {
        // TODO: Reuters endpoint not yet identified - commented out for now
        // var reutersTasks = _reutersService.FetchNewsByCategoryAsync(request.Category);
        var vnExpressTasks = _vnExpressService.FetchNewsByCategoryAsync(request.Category);

        // await Task.WhenAll(reutersTasks, vnExpressTasks);
        await vnExpressTasks;

        var allNews = new List<NewsEntity>();
        // allNews.AddRange(await reutersTasks);
        allNews.AddRange(await vnExpressTasks);

        return allNews;
    }
}
