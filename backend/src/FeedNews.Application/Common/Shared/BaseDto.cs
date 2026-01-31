using FeedNews.Domain.Constants;

namespace FeedNews.Application.Common.Shared;

public class BaseDto
{
    public required string TrackId { get; set; }
    public required string Language { get; set; } = LanguageConstants.DEFAULT_LANGUAGE;
}