namespace FeedNews.Application.Common.Services;

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string body, string trackId);
}