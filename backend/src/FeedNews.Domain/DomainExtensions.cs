using Microsoft.Extensions.DependencyInjection;

namespace FeedNews.Domain;

public static class DomainExtensions
{
    public static IServiceCollection AddDomain(
        this IServiceCollection services)
    {
        // Register domain services here
        return services;
    }
}