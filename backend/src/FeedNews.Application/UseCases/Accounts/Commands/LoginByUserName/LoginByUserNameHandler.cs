using System.Diagnostics;
using System.Text;
using FeedNews.Application.Common.Abstractions.Messaging;
using FeedNews.Application.Common.Repositories;
using FeedNews.Application.Common.Services;
using FeedNews.Application.Common.Shared;
using FeedNews.Application.UseCases.Accounts.Models;
using FeedNews.Domain.Entities;
using FeedNews.Domain.Enums;
using FeedNews.Domain.Exceptions.Base;
using FeedNews.Share.Logger;
using FeedNews.Share.Model.Response;
using Microsoft.Extensions.Logging;

namespace FeedNews.Application.UseCases.Accounts.Commands.LoginByUserName;

public class LoginByUserNameHandler(
    ILogger<LoginByUserNameHandler> logger,
    IAccountRepository accountRepository,
    ICustomLogger customLogger,
    IJwtTokenService jwtTokenService
) : ICommandHandler<LoginByUserNameCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(LoginByUserNameCommand request, CancellationToken cancellationToken)
    {
        StringBuilder builder = new("LoginByUserNameHandler.Handle");
        LogLevel level = LogLevel.Information;
        Stopwatch stop = Stopwatch.StartNew();
        try
        {
            Account account = await ValidateAsync(request);
            return Result.Success(new LoginResponse
            {
                AccessTokenResponse = new AccessTokenResponse
                {
                    RefreshToken = jwtTokenService.GenerateJwtToken(account),
                    AccessToken = jwtTokenService.GenerateJwtToken(account)
                },
                AccountResponse = new AccountResponse
                {
                    Id = account.Id,
                    Email = account.Email,
                    AvatarUrl = account.AvatarUrl,
                    FullName = account.FullName,
                    RoleName = ((Roles)account.RoleId).ToString()
                }
            }, request.TrackId);
        }
        catch (InvalidBusinessException e)
        {
            level = LogLevel.Warning;
            builder.Append($" InvalidBusinessException: {e.Message}");
            throw;
        }
        catch (Exception e)
        {
            level = LogLevel.Critical;
            builder.Append($" Exception: {e.Message}");
            throw;
        }
        finally
        {
            stop.Stop();
            await customLogger.WriteCustomLogAsync(logger, request.TrackId, builder.ToString(), string.Empty, level,
                stop.ElapsedMilliseconds);
        }
    }

    private async Task<Account> ValidateAsync(LoginByUserNameCommand request)
    {
        Account? account = accountRepository.Get(a =>
                a.Email == request.Email && a.Status == AccountStatus.Verify && request.Password == a.Password)
            .FirstOrDefault();
        if (account == null) throw new InvalidBusinessException(ErrorMessageCode.ACCOUNT_NOT_EXIST, request.Email);

        return account;
    }
}