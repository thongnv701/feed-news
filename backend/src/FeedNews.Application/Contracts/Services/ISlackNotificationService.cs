using FeedNews.Domain.Entities;

namespace FeedNews.Application.Contracts.Services;

public interface ISlackNotificationService
{
    Task<bool> SendNewsToSlackAsync(List<News> articles);
}
