using FeedNews.Domain.Entities;

namespace FeedNews.Application.Common.Services;

public interface IJwtTokenService
{
    string GenerateJwtToken(Account account);
}