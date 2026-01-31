namespace FeedNews.Application.Common.Services;

public interface ILanguageService
{
    string GetPreferredLanguage(string acceptLanguageHeader);
}