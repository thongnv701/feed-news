namespace FeedNews.Application.Configuration;

public class NewsConfiguration
{
    public string FetchTime { get; set; } = "18:00";
    
    public List<string> Categories { get; set; } = new();
    
    public int TopNewsPerCategory { get; set; } = 5;
    
    public int SummaryLengthMin { get; set; } = 200;
    
    public int SummaryLengthMax { get; set; } = 500;
}
