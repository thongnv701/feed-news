using System.Globalization;
using FeedNews.Application.Common.Services;
using FeedNews.Domain.Constants;
using Microsoft.AspNetCore.Http;

namespace FeedNews.Infrastructure.Services;

public class LanguageAccessor : BaseService, ILanguageAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LanguageAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string CurrentLanguage
    {
        get
        {
            HttpContext? context = _httpContextAccessor.HttpContext;
            if (context == null) return LanguageConstants.DEFAULT_LANGUAGE;

            string acceptLanguage = context.Request.Headers.AcceptLanguage.ToString();
            return ParseLanguage(acceptLanguage);
        }
    }

    public CultureInfo GetCultureInfo()
    {
        string culture = CurrentLanguage;
        return new CultureInfo(culture);
    }

    private string ParseLanguage(string acceptLanguageHeader)
    {
        if (string.IsNullOrEmpty(acceptLanguageHeader))
            return LanguageConstants.DEFAULT_LANGUAGE;

        // Basic parsing - extract first language code
        string firstLang = acceptLanguageHeader.Split(',')[0].Trim();
        return firstLang.Split('-')[0]; // Just the language part, not country
    }
}