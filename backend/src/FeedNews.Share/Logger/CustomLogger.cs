using System.Diagnostics;
using System.Text;
using FeedNews.Share.Extensions;
using FeedNews.Share.Utilities;
using Microsoft.Extensions.Logging;

namespace FeedNews.Share.Logger;

public class CustomLogger : ICustomLogger
{
    public async Task WriteCustomLogAsync<T>(ILogger<T> iLogger, string trackId, string logMsg, string hangoutMsg,
        LogLevel logLevel = LogLevel.Information, long responseTime = 0, long sMin = 0)
    {
        string slowLyWarn = Utility.GetPrefixSlowLyLog(responseTime, sMin);

        if (logLevel == LogLevel.Information && !string.IsNullOrEmpty(slowLyWarn)) logLevel = LogLevel.Warning;

        string responseTimeStr = responseTime > 0 ? $"[{Utility.ConvertMillisecondToHourMinSec(responseTime)}] " : " ";
        string stackInfoStr = string.Empty;

        if (logLevel != LogLevel.Information && logLevel != LogLevel.Warning)
        {
            StackTrace st = new(1, true);
            StackFrame[] stFrames = st.GetFrames();
            IEnumerable<string> stackTraceLogArr =
                stFrames.Where(x => x.ToString().Contains(".cs")).Select(x => x.ToString());
            stackInfoStr = $"stack: {stackTraceLogArr.ToJson()}";
        }

        string logMsgStr = new StringBuilder().Append(slowLyWarn)
            .Append(responseTimeStr)
            .Append('[')
            .Append(trackId)
            .Append("] ")
            .Append(logMsg)
            .Append(' ')
            .Append(stackInfoStr)
            .ToString();
        iLogger.Log(logLevel, logMsgStr);

        await Task.CompletedTask;
    }
}