using FeedNews.Application.Common.Abstractions.Messaging;
using FeedNews.Share.Model.Response;

namespace FeedNews.Application.UseCases.Accounts.Commands;

public class CustomerLoginCommand : ICommand<Result>
{
    public AccountLoginRequest? AccountLogin { get; set; }
}