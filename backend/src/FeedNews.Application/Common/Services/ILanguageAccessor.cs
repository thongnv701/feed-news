using System.Globalization;

namespace FeedNews.Application.Common.Services;

public interface ILanguageAccessor
{
    public string CurrentLanguage { get; }
    CultureInfo GetCultureInfo();
}