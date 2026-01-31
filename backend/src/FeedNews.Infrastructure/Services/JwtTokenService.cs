using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FeedNews.Application.Common.Repositories;
using FeedNews.Application.Common.Services;
using FeedNews.Domain.Configurations;
using FeedNews.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FeedNews.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService, IBaseService
{
    private readonly IOptions<JwtSettings> _options;
    private readonly IRoleRepository _roleRepository;

    public JwtTokenService(
        IRoleRepository roleRepository,
        IOptions<JwtSettings> options)
    {
        _roleRepository = roleRepository;
        _options = options;
    }

    public string GenerateJwtToken(Account account)
    {
        return GenerateJwtToken(account, _options.Value.TokenExpire);
    }

    private string GenerateJwtToken(Account account, int expireInMinutes)
    {
        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(_options.Value.SecretKey));
        SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256);

        Role role = _roleRepository.GetById(account.RoleId)
                    ?? throw new UnauthorizedAccessException($"Role id of account id: {account.Id} not correct");
        Claim[] claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, account.Email),
            new Claim(ClaimTypes.Name, account.FullName ?? string.Empty),
            new Claim(ClaimTypes.Role, role.Name)
        };

        JwtSecurityToken token = new(
            _options.Value.Issuer,
            _options.Value.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(expireInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}