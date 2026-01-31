namespace FeedNews.Application.UseCases.Accounts.Commands.RegisterByEmail;

public class RegisterByEmailInputDto
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
}