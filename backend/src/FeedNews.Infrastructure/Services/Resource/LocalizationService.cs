using FeedNews.Application.Common.Services;
using FeedNews.Domain.Constants;
using FeedNews.Infrastructure.Common.Resources;

namespace FeedNews.Infrastructure.Services.Resource;

internal class LocalizationService(
    ILanguageAccessor languageAccessor
) : ILocalizationService
{
    private readonly string LogoUrlKey = "{logo_url}";
    private readonly string LogoUrlValue = UrlConstants.LOGO_URL;
    private readonly string SupportEmailKey = "{support_email}";
    private readonly string SupportEmailValue = "thongnv701@gmail.com";
    private readonly string WebUrlKey = "{web_url}";
    private readonly string WebUrlvalue = UrlConstants.HOME_URL;

    public string GetMessageErrorString(string key, params object[] args)
    {
        if (string.IsNullOrEmpty(key))
            return "Key cannot be null or empty";

        try
        {
            string? value = MessageErrors.ResourceManager.GetString(key, languageAccessor.GetCultureInfo());
            // Check if the key was found or not
            if (value == null) return $"Resource not found for key: {key}";

            return GetValueAfterReplace(value, args);
        }
        catch (Exception ex)
        {
            return $"Error retrieving resource: {ex.Message}";
        }
    }

    public string GetEmailString(string key, params object[] args)
    {
        if (string.IsNullOrEmpty(key))
            return "Key cannot be null or empty";

        try
        {
            string? value = EmailTemplates.ResourceManager.GetString(key, languageAccessor.GetCultureInfo());
            // Check if the key was found or not
            if (value == null) return $"Resource not found for key: {key}";

            return GetValueAfterReplace(value, args);
        }
        catch (Exception ex)
        {
            return $"Error retrieving resource: {ex.Message}";
        }
    }

    private string GetValueAfterReplace(string value, params object[] args)
    {
        // Format the string with the provided arguments
        for (int i = 0; i < args.Length; i++) value = value.Replace($"{{{i}}}", args[i]?.ToString() ?? string.Empty);

        // Auto replace email and web url
        value = value.Replace(SupportEmailKey, SupportEmailValue);
        value = value.Replace(WebUrlKey, WebUrlvalue);
        value = value.Replace(LogoUrlKey, LogoUrlValue);
        return value;
    }
}