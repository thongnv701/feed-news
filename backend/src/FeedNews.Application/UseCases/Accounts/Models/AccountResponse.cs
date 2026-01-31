namespace FeedNews.Application.UseCases.Accounts.Models;

public class AccountResponse
{
    public long Id { get; set; }

    public string? FullName { get; set; }

    public string? LastName { get; set; }

    public string? RoleName { get; set; }

    public string? Email { get; set; }

    public string? AvatarUrl { get; set; }
}