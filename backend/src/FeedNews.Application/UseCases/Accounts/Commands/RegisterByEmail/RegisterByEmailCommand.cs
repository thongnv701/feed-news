using FeedNews.Application.Common.Abstractions.Messaging;
using FeedNews.Application.Common.Shared;

namespace FeedNews.Application.UseCases.Accounts.Commands.RegisterByEmail;

public class RegisterByEmailCommand : BaseDto, ICommand<RegisterByEmailOutputDto>
{
    public required RegisterByEmailInputDto Input { get; set; }
}