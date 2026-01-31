using FeedNews.Domain.Entities;

namespace FeedNews.Infrastructure.Services.Abstractions;

public interface ISlackNotificationService
{
    Task<bool> SendNewsToSlackAsync(List<News> articles);
}
