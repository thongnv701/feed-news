using System.Globalization;
using FeedNews.Application.Common.Services;
using FeedNews.Domain.Constants;

namespace FeedNews.Infrastructure.Services;

public class LanguageService : BaseService, ILanguageService
{
    private readonly string _defaultLanguage = LanguageConstants.DEFAULT_LANGUAGE;

    public string GetPreferredLanguage(string acceptLanguageHeader)
    {
        if (string.IsNullOrEmpty(acceptLanguageHeader))
            return _defaultLanguage;

        // Parse the Accept-Language header
        var languages = acceptLanguageHeader.Split(',')
            .Select(part =>
            {
                string[] parts = part.Trim().Split(';');
                string language = parts[0].Trim();
                double quality = parts.Length > 1 && parts[1].StartsWith("q=")
                    ? double.Parse(parts[1].Substring(2), CultureInfo.InvariantCulture)
                    : 1.0;
                return new { Language = language, Quality = quality };
            })
            .OrderByDescending(l => l.Quality);

        // Find the first supported language
        foreach (var language in languages)
        {
            string languageCode = language.Language.Split('-')[0].ToLowerInvariant();
            if (LanguageConstants.LANGUAGES_SUPPORT.Contains(languageCode))
                return languageCode;
        }

        return _defaultLanguage;
    }
}