using MediatR;
using NewsEntity = FeedNews.Domain.Entities.News;

namespace FeedNews.Application.Features.News.Commands;

public record SendNewsToSlackCommand(List<NewsEntity> Articles) : IRequest<bool>;
