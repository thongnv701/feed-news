using FeedNews.Application.Common.Abstractions.Messaging;
using FeedNews.Application.UseCases.Accounts.Models;

namespace FeedNews.Application.UseCases.Accounts.Commands.LoginByUserName;

public class LoginByUserNameCommand : LoginByUserNameDto, ICommand<LoginResponse>
{
    public required string TrackId { get; set; }
}