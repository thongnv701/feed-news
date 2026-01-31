using FeedNews.Application.Contracts.Services;
using MediatR;

namespace FeedNews.Application.Features.News.Commands;

public class SendNewsToSlackCommandHandler : IRequestHandler<SendNewsToSlackCommand, bool>
{
    private readonly ISlackNotificationService _slackService;

    public SendNewsToSlackCommandHandler(ISlackNotificationService slackService)
    {
        _slackService = slackService;
    }

    public async Task<bool> Handle(SendNewsToSlackCommand request, CancellationToken cancellationToken)
    {
        var result = await _slackService.SendNewsToSlackAsync(request.Articles);
        return result;
    }
}
