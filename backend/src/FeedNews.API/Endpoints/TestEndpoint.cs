using Asp.Versioning;
using Asp.Versioning.Builder;
using Carter;
using CorrelationId.Abstractions;
using FeedNews.Application.UseCases.Test.Queries.TestError;
using FeedNews.Share.Model.Response;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FeedNews.API.Endpoints;

public class TestEndpoint : BaseEndpoint, ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        ApiVersion currentVersion = new(1, 0);
        ApiVersionSet apiVersionSet = app
            .NewApiVersionSet()
            .HasApiVersion(currentVersion)
            .ReportApiVersions()
            .Build();
        RouteGroupBuilder builderGroup = app.MapGroup("");

        builderGroup.MapGet("api/v1/ping",
            async (CancellationToken cancellationToken) => { return TypedResults.Ok(new { Message = "Pong" }); });

        builderGroup.MapGet("/",
            async (CancellationToken cancellationToken) => { return TypedResults.Ok(new { Message = "Pong" }); });


        builderGroup.MapGet("/test", async ([FromServices] ISender mediator, [FromServices] ICorrelationContextAccessor correlationContextAccessor, CancellationToken cancellationToken) =>
        {
            Result<string> result = await mediator.Send(new GetTestErrorQuery
            {
                TrackId = correlationContextAccessor.CorrelationContext.CorrelationId,
                Language = "en"
            }, cancellationToken);
            return HandleResult(result);
        });
    }
}