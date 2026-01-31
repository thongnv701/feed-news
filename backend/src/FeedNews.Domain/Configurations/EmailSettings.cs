namespace FeedNews.Domain.Configurations;

public class EmailSettings
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string SmtpClient { get; set; }
}