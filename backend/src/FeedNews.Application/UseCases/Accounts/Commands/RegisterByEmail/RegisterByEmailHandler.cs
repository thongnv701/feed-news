using System.Diagnostics;
using System.Text;
using FeedNews.Application.Common.Abstractions.Messaging;
using FeedNews.Application.Common.Repositories;
using FeedNews.Application.Common.Services;
using FeedNews.Application.Common.Shared;
using FeedNews.Domain.Constants;
using FeedNews.Domain.Entities;
using FeedNews.Domain.Enums;
using FeedNews.Domain.Exceptions.Base;
using FeedNews.Domain.Extensions;
using FeedNews.Share.Extensions;
using FeedNews.Share.Logger;
using FeedNews.Share.Model.Response;
using Microsoft.Extensions.Logging;

namespace FeedNews.Application.UseCases.Accounts.Commands.RegisterByEmail;

public class RegisterByEmailHandler : ICommandHandler<RegisterByEmailCommand, RegisterByEmailOutputDto>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICacheService _cacheService;
    private readonly ICustomLogger _customLogger;
    private readonly IEmailService _emailService;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger<RegisterByEmailHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterByEmailHandler(IUnitOfWork unitOfWork,
        IAccountRepository accountRepository,
        IEmailService emailService,
        ILocalizationService localizationService,
        ICacheService cacheService,
        ICustomLogger customLogger,
        ILogger<RegisterByEmailHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _accountRepository = accountRepository;
        _emailService = emailService;
        _localizationService = localizationService;
        _cacheService = cacheService;
        _customLogger = customLogger;
        _logger = logger;
    }

    public async Task<Result<RegisterByEmailOutputDto>> Handle(RegisterByEmailCommand request,
        CancellationToken cancellationToken)
    {
        StringBuilder builder = new("RegisterByEmailHandler.Handle");
        LogLevel level = LogLevel.Information;
        Stopwatch stop = Stopwatch.StartNew();
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // var accountInDb = await ValidateAsync(request);
            // if (accountInDb == null)
            // {
            //     var account = new Account()
            //     {
            //         Email = request.Input.Email,
            //         Password = request.Input.Password,
            //         FullName = request.Input.FullName,
            //         RoleId = (int)Roles.Customer,
            //         Status = AccountStatus.UnVerify,
            //         Type = AccountTypes.Local
            //     };
            //
            //     await _accountRepository.AddAsync(account);
            //     await _unitOfWork.CommitTransactionAsync();
            // }
            //
            // var bodyRequest = await BodyEmailAsync(request);
            // // Send an email to verify the account
            // _emailService.SendEmailAsync(
            //     request.Input.Email,
            //     _localizationService.GetEmailString(EmailTemplateCode.VERIFICATION_EMAIL_SUBJECT),
            //     bodyRequest,
            //     request.TrackId);

            return Result.Success(new RegisterByEmailOutputDto(), request.TrackId);
        }
        catch (InvalidBusinessException e)
        {
            level = LogLevel.Warning;
            builder.Append($" Error: {e.Source}, Message: {e.Message}");
            throw;
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            level = LogLevel.Critical;
            builder.Append($" Exception: {e.Message}").Append($" stackTrace: {e.StackTrace}");
            throw;
        }
        finally
        {
            builder.Append($" Request: {{ {request.Input.ToJson()} }}");
            stop.Stop();
            await _customLogger.WriteCustomLogAsync(_logger, request.TrackId, builder.ToString(), string.Empty, level,
                stop.ElapsedMilliseconds);
        }
    }

    private async Task<Account?> ValidateAsync(RegisterByEmailCommand request)
    {
        // Check if account already exist
        Account? account = (await _accountRepository.GetAsync(x => x.Email == request.Input.Email)).SingleOrDefault();
        if (account is not null && account.Status != AccountStatus.UnVerify)
            throw new InvalidBusinessException(ErrorMessageCode.ACCOUNT_EXIST, request.Input.Email);

        return account;
    }

    private async Task<string> BodyEmailAsync(RegisterByEmailCommand request)
    {
        int verifyCode =
            await _cacheService.GenerateAndSaveUniqueSixDigitCodeAsync(
                AuthExtensions.GetRedisVerifyCodeKey(request.Input.Email), TimeSpan.FromMinutes(10));
        // Get the email template based on language
        string emailTemplate = _localizationService.GetEmailString(EmailTemplateCode.VERIFICATION_EMAIL_BODY,
            request.Input.FullName,
            verifyCode,
            $"{UrlConstants.HOME_URL}?email={request.Input.Email}&token={verifyCode}");

        return emailTemplate;
    }
}