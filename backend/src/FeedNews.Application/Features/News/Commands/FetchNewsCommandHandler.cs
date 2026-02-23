using FeedNews.Application.Common.Repositories;
using FeedNews.Application.Contracts.Repositories;
using FeedNews.Application.Contracts.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using NewsEntity = FeedNews.Domain.Entities.News;

namespace FeedNews.Application.Features.News.Commands;

public class FetchNewsCommandHandler : IRequestHandler<FetchNewsCommand, List<NewsEntity>>
{
    private readonly IReutersNewsService _reutersService;
    private readonly IVNExpressNewsService _vnExpressService;
    private readonly INewsRepository _newsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FetchNewsCommandHandler> _logger;

    public FetchNewsCommandHandler(
        IReutersNewsService reutersService,
        IVNExpressNewsService vnExpressService,
        INewsRepository newsRepository,
        IUnitOfWork unitOfWork,
        ILogger<FetchNewsCommandHandler> logger)
    {
        _reutersService = reutersService;
        _vnExpressService = vnExpressService;
        _newsRepository = newsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
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

        var uniqueNews = new List<NewsEntity>();
        foreach (var news in allNews)
        {
            var exists = await _newsRepository.ExistsByUrlAsync(news.Url);
            if (!exists)
            {
                await _newsRepository.AddAsync(news);
                uniqueNews.Add(news);
            }
        }

        // Batch save all new articles to database (Skip Failed strategy)
        if (uniqueNews.Any())
        {
            try
            {
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully persisted {Count} articles to database", uniqueNews.Count);
            }
            catch (Exception ex)
            {
                // Error handling: Log error but continue (Skip Failed strategy per requirements)
                _logger.LogError(ex, "Error persisting {Count} articles to database. Pipeline will continue with cached data.", uniqueNews.Count);
            }
        }

        return uniqueNews;
    }
}
