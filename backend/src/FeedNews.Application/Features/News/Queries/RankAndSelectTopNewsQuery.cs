using FeedNews.Domain.Enums;
using MediatR;
using NewsEntity = FeedNews.Domain.Entities.News;

namespace FeedNews.Application.Features.News.Queries;

public record RankAndSelectTopNewsQuery(NewsCategory Category, List<NewsEntity> AllNews) : IRequest<List<NewsEntity>>;
