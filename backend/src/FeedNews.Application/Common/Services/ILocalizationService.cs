namespace FeedNews.Application.Common.Services;

public interface ILocalizationService
{
    string GetMessageErrorString(string key, params object[] args);
    string GetEmailString(string key, params object[] args);
}