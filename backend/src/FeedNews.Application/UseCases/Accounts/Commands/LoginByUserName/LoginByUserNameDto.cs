namespace FeedNews.Application.UseCases.Accounts.Commands.LoginByUserName;

public class LoginByUserNameDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}