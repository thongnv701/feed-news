using Asp.Versioning;
using Asp.Versioning.Builder;
using Carter;
using CorrelationId.Abstractions;
using FeedNews.Application.Common.Services;
using FeedNews.Application.UseCases.Accounts.Commands.LoginByUserName;
using FeedNews.Application.UseCases.Accounts.Commands.RegisterByEmail;
using FeedNews.Application.UseCases.Accounts.Commands.VerifyByCode;
using FeedNews.Application.UseCases.Accounts.Models;
using FeedNews.Share.Model.Response;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FeedNews.API.Endpoints;

public class AccountEndpoint : BaseEndpoint, ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        ApiVersion currentVersion = new(1, 0);
        ApiVersionSet apiVersionSet = app
            .NewApiVersionSet()
            .HasApiVersion(currentVersion)
            .ReportApiVersions()
            .Build();
        RouteGroupBuilder builderGroup = app.MapGroup("api/v{apiVersion:apiVersion}/auth")
            .WithApiVersionSet(apiVersionSet);

        builderGroup.MapGet("/", async (CancellationToken cancellationToken) => { return TypedResults.Ok(new { }); });

        #region Post

        builderGroup.MapPost("register-legacy", async (
            [FromBody] RegisterByEmailInputDto request,
            [FromServices] ISender mediator,
            [FromServices] ICorrelationContextAccessor correlationContextAccessor,
            [FromServices] ILanguageAccessor languageAccessor,
            CancellationToken cancellationToken) =>
        {
            RegisterByEmailCommand command = new()
            {
                Input = request,
                TrackId = correlationContextAccessor.CorrelationContext.CorrelationId,
                Language = languageAccessor.CurrentLanguage
            };
            Result<RegisterByEmailOutputDto> result = await mediator.Send(command, cancellationToken);
            return HandleResult(result);
        });

        builderGroup.MapPost("verify", async (
            [FromBody] VerifyByCodeDto request,
            [FromServices] ISender mediator,
            [FromServices] ICorrelationContextAccessor correlationContextAccessor,
            CancellationToken cancellationToken) =>
        {
            VerifyByCodeCommand command = request.Adapt<VerifyByCodeCommand>();
            command.TrackId = correlationContextAccessor.CorrelationContext.CorrelationId;
            Result<LoginResponse> result = await mediator.Send(command, cancellationToken);
            return HandleResult(result);
        });

        builderGroup.MapPost("login", async (
            [FromBody] LoginByUserNameDto request,
            [FromServices] ISender mediator,
            [FromServices] ICorrelationContextAccessor correlationContextAccessor,
            CancellationToken cancellationToken) =>
        {
            LoginByUserNameCommand command = request.Adapt<LoginByUserNameCommand>();
            command.TrackId = correlationContextAccessor.CorrelationContext.CorrelationId;
            Result<LoginResponse> result = await mediator.Send(command, cancellationToken);
            return HandleResult(result);
        });

        #endregion
    }
}