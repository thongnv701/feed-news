namespace FeedNews.Application.Configuration;

public class GeminiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    
    public string Model { get; set; } = "gemini-1.5-flash";
    
    public int MaxTokens { get; set; } = 500;
}
