namespace FeedNews.Application.UseCases.Accounts.Models;

public class AccessTokenResponse
{
    public required string AccessToken { get; set; }

    public required string RefreshToken { get; set; }
}