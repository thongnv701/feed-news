using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FeedNews.Infrastructure.Services
{
    /// <summary>
    /// Slack Incoming Webhook logger for real-time log messages to Slack channel
    /// </summary>
    public class SlackWebhookLogger : IDisposable
    {
        private readonly string _webhookUrl;
        private readonly LogLevel _minimumLogLevel;
        private readonly HttpClient _httpClient;
        private readonly bool _includeException;
        private readonly bool _includeTimestamp;

        public SlackWebhookLogger(
            string webhookUrl,
            LogLevel minimumLogLevel = LogLevel.Warning,
            bool includeException = true,
            bool includeTimestamp = true)
        {
            _webhookUrl = webhookUrl ?? throw new ArgumentNullException(nameof(webhookUrl));
            _minimumLogLevel = minimumLogLevel;
            _httpClient = new HttpClient();
            _includeException = includeException;
            _includeTimestamp = includeTimestamp;
        }

        /// <summary>
        /// Send a log message to Slack via webhook
        /// </summary>
        public async Task LogToSlackAsync(
            LogLevel logLevel,
            string category,
            string message,
            Exception? exception = null)
        {
            if (logLevel < _minimumLogLevel)
                return;

            try
            {
                var color = GetColorForLogLevel(logLevel);
                var emoji = GetEmojiForLogLevel(logLevel);

                var payload = new
                {
                    text = $"{emoji} {logLevel}",
                    attachments = new[]
                    {
                        new
                        {
                            color,
                            title = category,
                            text = message,
                            ts = _includeTimestamp ? DateTimeOffset.UtcNow.ToUnixTimeSeconds() : (long?)null,
                            fields = _includeException && exception != null ? new[]
                            {
                                new
                                {
                                    title = "Exception Type",
                                    value = exception.GetType().Name,
                                    @short = true
                                },
                                new
                                {
                                    title = "Exception Message",
                                    value = exception.Message,
                                    @short = false
                                },
                                new
                                {
                                    title = "Stack Trace",
                                    value = $"```{exception.StackTrace}```",
                                    @short = false
                                }
                            } : Array.Empty<object>()
                        }
                    }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_webhookUrl, content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                // Log locally if Slack logging fails - don't throw
                System.Diagnostics.Debug.WriteLine($"Failed to send log to Slack: {ex.Message}");
            }
        }

        /// <summary>
        /// Get Slack message color based on log level
        /// </summary>
        private static string GetColorForLogLevel(LogLevel logLevel) => logLevel switch
        {
            LogLevel.Critical => "#FF0000",      // Red
            LogLevel.Error => "#FF6B6B",         // Light Red
            LogLevel.Warning => "#FFA500",       // Orange
            LogLevel.Information => "#0099FF",   // Blue
            LogLevel.Debug => "#999999",         // Gray
            _ => "#CCCCCC"                       // Light Gray
        };

        /// <summary>
        /// Get emoji for log level
        /// </summary>
        private static string GetEmojiForLogLevel(LogLevel logLevel) => logLevel switch
        {
            LogLevel.Critical => "🚨",
            LogLevel.Error => "❌",
            LogLevel.Warning => "⚠️",
            LogLevel.Information => "ℹ️",
            LogLevel.Debug => "🔍",
            _ => "📝"
        };

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
