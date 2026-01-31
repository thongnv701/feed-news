using MediatR;
using NewsEntity = FeedNews.Domain.Entities.News;

namespace FeedNews.Application.Features.News.Commands;

public record GenerateSummaryCommand(NewsEntity News) : IRequest<NewsEntity>;
