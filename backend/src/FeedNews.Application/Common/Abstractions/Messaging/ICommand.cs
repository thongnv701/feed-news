using FeedNews.Share.Model.Response;
using MediatR;

namespace FeedNews.Application.Common.Abstractions.Messaging;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}