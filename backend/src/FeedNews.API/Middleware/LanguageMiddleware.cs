using FeedNews.Application.Common.Services;
using FeedNews.Domain.Constants;

namespace FeedNews.API.Middleware;

public class LanguageMiddleware
{
    private readonly ILanguageService _languageService;
    private readonly RequestDelegate _next;

    public LanguageMiddleware(RequestDelegate next, ILanguageService languageService)
    {
        _next = next;
        _languageService = languageService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract the Accept-Language header
        string acceptLanguageHeader = context.Request.Headers.AcceptLanguage.ToString();

        // Determine the preferred language
        string language = _languageService.GetPreferredLanguage(acceptLanguageHeader);

        // Store the language in the HttpContext items for later use
        context.Items[LanguageConstants.KEY_GET_LANGUEGE] = language;

        await _next(context);
    }
}

// Extension method for middleware registration
public static class LanguageMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<LanguageMiddleware>();
    }
}