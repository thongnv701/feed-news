namespace FeedNews.Application.Configuration;

public class NewsFeedsConfiguration
{
    public RssFeeds Reuters { get; set; } = new();
    
    public RssFeeds VNExpress { get; set; } = new();
}

public class RssFeeds
{
    public Dictionary<string, string> RssFeedUrls { get; set; } = new();
}
