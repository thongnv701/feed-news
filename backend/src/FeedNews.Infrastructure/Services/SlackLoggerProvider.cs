using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace FeedNews.Infrastructure.Services
{
    /// <summary>
    /// ASP.NET Core Logger Provider for Slack Webhook
    /// </summary>
    public class SlackLoggerProvider : ILoggerProvider
    {
        private readonly SlackWebhookLogger _slackLogger;
        private readonly ConcurrentDictionary<string, SlackLoggerImpl> _loggers = new();

        public SlackLoggerProvider(
            string webhookUrl,
            LogLevel minimumLogLevel = LogLevel.Warning,
            bool includeException = true,
            bool includeTimestamp = true)
        {
            _slackLogger = new SlackWebhookLogger(webhookUrl, minimumLogLevel, includeException, includeTimestamp);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new SlackLoggerImpl(name, _slackLogger));
        }

        public void Dispose()
        {
            _slackLogger?.Dispose();
        }

        /// <summary>
        /// Internal logger implementation
        /// </summary>
        private class SlackLoggerImpl : ILogger
        {
            private readonly string _categoryName;
            private readonly SlackWebhookLogger _slackLogger;

            public SlackLoggerImpl(string categoryName, SlackWebhookLogger slackLogger)
            {
                _categoryName = categoryName;
                _slackLogger = slackLogger;
            }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return logLevel != LogLevel.None;
            }

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel))
                    return;

                var message = formatter(state, exception);
                
                // Fire and forget - don't await
                _ = _slackLogger.LogToSlackAsync(logLevel, _categoryName, message, exception);
            }
        }
    }
}
