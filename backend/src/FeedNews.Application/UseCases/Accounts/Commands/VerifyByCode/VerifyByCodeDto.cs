namespace FeedNews.Application.UseCases.Accounts.Commands.VerifyByCode;

public class VerifyByCodeDto
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}