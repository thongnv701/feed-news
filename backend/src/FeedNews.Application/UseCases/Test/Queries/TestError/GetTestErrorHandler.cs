using FeedNews.Application.Common.Abstractions.Messaging;
using FeedNews.Application.Common.Services;
using FeedNews.Application.Common.Shared;
using FeedNews.Domain.Constants;
using FeedNews.Share.Model.Response;
using Microsoft.Extensions.Logging;

namespace FeedNews.Application.UseCases.Test.Queries.TestError;

public class GetTestErrorHandler(
    ILogger<GetTestErrorHandler> logger,
    ILocalizationService localizationService
) : IQueryHandler<GetTestErrorQuery, string>
{
    public async Task<Result<string>> Handle(GetTestErrorQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"[{request.TrackId}] GetTestErrorHandler.Handle");
        // cacheService.SetCacheResponseAsync("TestError", "TestError", TimeSpan.FromMinutes(1));
        // throw new InvalidBusinessException(ErrorMessageCode.TEST);
        string emailTemplate = await BodyEmailAsync();
        Result<string> result = Result.Success(emailTemplate, request.TrackId);
        return result;
    }

    private async Task<string> BodyEmailAsync()
    {
        // Get the email template based on language
        string emailTemplate = localizationService.GetEmailString(EmailTemplateCode.VERIFICATION_EMAIL_BODY,
            UrlConstants.HOME_URL,
            "Thong",
            "1234",
            "");

        return emailTemplate;
    }
}