using FeedNews.Share.Model.Response;
using MediatR;

namespace FeedNews.Application.Common.Abstractions.Messaging;

public interface IQueryHandler<TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}