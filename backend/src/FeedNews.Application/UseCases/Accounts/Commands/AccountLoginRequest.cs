namespace FeedNews.Application.UseCases.Accounts.Commands;

public sealed record AccountLoginRequest(string Email, string Password);