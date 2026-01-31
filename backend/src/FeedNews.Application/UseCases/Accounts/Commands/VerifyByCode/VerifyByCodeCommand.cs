using FeedNews.Application.Common.Abstractions.Messaging;
using FeedNews.Application.UseCases.Accounts.Models;

namespace FeedNews.Application.UseCases.Accounts.Commands.VerifyByCode;

public class VerifyByCodeCommand : VerifyByCodeDto, ICommand<LoginResponse>
{
    public required string TrackId { get; set; }
}