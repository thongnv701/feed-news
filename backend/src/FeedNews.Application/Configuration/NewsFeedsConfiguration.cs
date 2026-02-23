namespace FeedNews.Application.Configuration;

public class NewsFeedsConfiguration
{
    public RssFeeds Reuters { get; set; } = new();
    
    public RssFeeds VNExpress { get; set; } = new();
    
    /// <summary>
    /// Configurable maximum number of articles to fetch per category.
    /// If a category is not specified, uses the "Default" value.
    /// </summary>
    public Dictionary<string, int> MaxArticlesPerFetch { get; set; } = new()
    {
        { "Default", 5 }
    };
}

public class RssFeeds
{
    public Dictionary<string, string> RssFeedUrls { get; set; } = new();
}
