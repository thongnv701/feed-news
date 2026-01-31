using Microsoft.Extensions.Logging;

namespace FeedNews.Share.Logger;

public interface ICustomLogger
{
    Task WriteCustomLogAsync<T>(ILogger<T> iLogger, string trackId, string logMsg, string hangoutMsg,
        LogLevel logLevel = LogLevel.Information, long responseTime = 0, long sMin = 0);
}