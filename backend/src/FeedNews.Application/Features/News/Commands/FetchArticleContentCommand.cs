using MediatR;
using NewsEntity = FeedNews.Domain.Entities.News;

namespace FeedNews.Application.Features.News.Commands;

/// <summary>
/// Command to fetch full article content from a News article's URL
/// </summary>
public record FetchArticleContentCommand(NewsEntity News) : IRequest<NewsEntity>;
