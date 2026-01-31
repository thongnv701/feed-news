using FeedNews.Domain.Enums;
using MediatR;
using NewsEntity = FeedNews.Domain.Entities.News;

namespace FeedNews.Application.Features.News.Commands;

public record FetchNewsCommand(NewsCategory Category) : IRequest<List<NewsEntity>>;
