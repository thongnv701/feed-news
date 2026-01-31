namespace FeedNews.Application.UseCases.Accounts.Models;

public class LoginResponse
{
    public required AccountResponse AccountResponse { get; set; }
    public required AccessTokenResponse AccessTokenResponse { get; set; }
}