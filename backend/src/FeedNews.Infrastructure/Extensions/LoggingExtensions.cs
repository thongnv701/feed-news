using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FeedNews.Infrastructure.Services;

namespace FeedNews.Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for configuring Slack webhook logging
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        /// Add Slack webhook logging provider to the logging factory
        /// </summary>
        public static ILoggingBuilder AddSlackWebhook(
            this ILoggingBuilder builder,
            string webhookUrl,
            LogLevel minimumLogLevel = LogLevel.Warning,
            bool includeException = true,
            bool includeTimestamp = true)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl))
                throw new ArgumentException("Webhook URL cannot be null or empty", nameof(webhookUrl));

            builder.AddProvider(new SlackLoggerProvider(webhookUrl, minimumLogLevel, includeException, includeTimestamp));
            return builder;
        }

        /// <summary>
        /// Add Slack webhook logging provider from configuration
        /// </summary>
        public static ILoggingBuilder AddSlackWebhookFromConfig(
            this ILoggingBuilder builder,
            IConfiguration configuration)
        {
            var webhookUrl = configuration?["Logging:SlackWebhook:WebhookUrl"];
            if (string.IsNullOrWhiteSpace(webhookUrl))
                throw new ArgumentException("SlackWebhook:WebhookUrl not configured", nameof(webhookUrl));

            var minimumLogLevelStr = configuration?["Logging:SlackWebhook:MinimumLogLevel"] ?? "Warning";
            var includeException = configuration?["Logging:SlackWebhook:IncludeException"] == "true";
            var includeTimestamp = configuration?["Logging:SlackWebhook:IncludeTimestamp"] != "false";

            if (!Enum.TryParse<LogLevel>(minimumLogLevelStr, out var minimumLogLevel))
                minimumLogLevel = LogLevel.Warning;

            return builder.AddSlackWebhook(webhookUrl, minimumLogLevel, includeException, includeTimestamp);
        }
    }
}
