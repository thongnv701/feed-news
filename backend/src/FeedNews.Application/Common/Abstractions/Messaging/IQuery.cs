using FeedNews.Share.Model.Response;
using MediatR;

namespace FeedNews.Application.Common.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}