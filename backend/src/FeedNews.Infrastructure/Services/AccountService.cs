using FeedNews.Application.Common.Services;
using FeedNews.Share.Utilities;
using Microsoft.Extensions.Logging;

namespace FeedNews.Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly ILogger<AccountService> logger;

    public AccountService(ILogger<AccountService> logger)
    {
        this.logger = logger;
    }

    public void TestWriteLog()
    {
        logger.LogInformation($"password: {BCrypUtility.Hash("1")}");
        logger.LogInformation("Log successly");
    }
}