using FeedNews.Application.Common.Abstractions.Messaging;
using FeedNews.Domain.Entities;
using FeedNews.Share.Model.Response;

namespace FeedNews.Application.UseCases.Accounts.Queries;

public class GetAllAccountQuery : IQuery<Result<List<Account>>>
{
}