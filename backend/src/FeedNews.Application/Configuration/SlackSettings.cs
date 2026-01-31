namespace FeedNews.Application.Configuration;

public class SlackSettings
{
    public string BotToken { get; set; } = string.Empty;
    
    public string ChannelId { get; set; } = string.Empty;
}
