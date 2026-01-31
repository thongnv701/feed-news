using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Text;
using FeedNews.Application.Common.Services;
using FeedNews.Domain.Configurations;
using FeedNews.Share.Logger;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FeedNews.Infrastructure.Services;

public class EmailService(
    ILogger<EmailService> logger,
    ICustomLogger customLogger,
    IOptions<EmailSettings> options
) : BaseService, IEmailService
{
    public async Task SendEmailAsync(string email, string subject, string body, string trackId)
    {
        StringBuilder builder = new($"EmailService.SendEmailAsync Email:${email} Subject:${subject}");
        LogLevel level = LogLevel.Information;
        Stopwatch stop = Stopwatch.StartNew();
        try
        {
            MailMessage message = new()
            {
                From = new MailAddress(options.Value.Email, "FeedNews"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(email));

            SmtpClient smtpClient = new(options.Value.SmtpClient)
            {
                Port = 587,
                Credentials = new NetworkCredential(options.Value.Email, options.Value.Password),
                EnableSsl = true
            };

            smtpClient.Send(message);
        }
        catch (Exception ex)
        {
            level = LogLevel.Critical;
            builder.Append($" Exception: {ex.Message}").Append($" stackTrace: {ex.StackTrace}");
            throw;
        }
        finally
        {
            stop.Stop();
            await customLogger.WriteCustomLogAsync(logger, trackId, builder.ToString(), string.Empty, level,
                stop.ElapsedMilliseconds);
        }
    }
}