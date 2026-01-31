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
using FeedNews.Domain.Extensions;
using FeedNews.Share.Logger;
using FeedNews.Share.Model.Response;
using Microsoft.Extensions.Logging;

namespace FeedNews.Application.UseCases.Accounts.Commands.VerifyByCode;

public class VerifyByCodeHandler(
    ILogger<VerifyByCodeHandler> logger,
    ICustomLogger customLogger,
    IUnitOfWork unitOfWork,
    IAccountRepository accountResRepository,
    ICacheService cacheService,
    IJwtTokenService jwtTokenService
) : ICommandHandler<VerifyByCodeCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(VerifyByCodeCommand request, CancellationToken cancellationToken)
    {
        StringBuilder builder = new("VerifyByCodeHandler.Handle");
        LogLevel level = LogLevel.Information;
        Stopwatch stop = Stopwatch.StartNew();
        await unitOfWork.BeginTransactionAsync();
        try
        {
            Account account = await ValidateAsync(request);
            account.Status = AccountStatus.Verify;
            accountResRepository.Update(account);
            await unitOfWork.CommitTransactionAsync();

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
            builder.Append($", Error: {e.Source}, Message: {e.Message}");
            throw;
        }
        catch (Exception e)
        {
            unitOfWork.RollbackTransaction();
            level = LogLevel.Critical;
            builder.Append($", Exception: {e.Message}, StackTrace: {e.StackTrace}");
            throw;
        }
        finally
        {
            stop.Stop();
            await customLogger.WriteCustomLogAsync(
                logger,
                request.TrackId,
                builder.ToString(),
                string.Empty,
                level,
                stop.ElapsedMilliseconds
            );
        }
    }

    private async Task<Account> ValidateAsync(VerifyByCodeCommand request)
    {
        Account? account = accountResRepository.Get(a => a.Email == request.Email && a.Status == AccountStatus.UnVerify)
            .FirstOrDefault();
        if (account == null) throw new InvalidBusinessException(ErrorMessageCode.ACCOUNT_NOT_EXIST, request.Email);

        string? code = await cacheService.GetCachedResponseAsync(AuthExtensions.GetRedisVerifyCodeKey(request.Email));
        if (code == null) throw new InvalidBusinessException(ErrorMessageCode.VERIFICATION_CODE_EXPIRED);

        if (code != request.Code) throw new InvalidBusinessException(ErrorMessageCode.VERIFICATION_CODE_NOT_MATCH);

        return account;
    }
}