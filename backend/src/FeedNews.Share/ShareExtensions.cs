using FeedNews.Share.Logger;
using Microsoft.Extensions.DependencyInjection;

namespace FeedNews.Share;

public static class ShareExtensions
{
    public static IServiceCollection AddShare(
        this IServiceCollection services)
    {
        services.AddSingleton<ICustomLogger, CustomLogger>();
        return services;
    }
}