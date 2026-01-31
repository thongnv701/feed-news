using System.Net;
using System.Text.Json;
using CorrelationId.Abstractions;
using FluentValidation;
using FeedNews.Application.Common.Enums;
using FeedNews.Application.Common.Exceptions;
using FeedNews.Application.Common.Services;
using FeedNews.Application.Shared;
using FeedNews.Domain.Enums;
using FeedNews.Domain.Exceptions.Base;
using FeedNews.Share.Model.Response;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace FeedNews.API.Middleware;

public class ExceptionMiddleware
{
    private readonly ICorrelationContextAccessor _correlationContextAccessor;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        ILocalizationService localizationService,
        ICorrelationContextAccessor correlationContextAccessor
    )
    {
        _next = next;
        _logger = logger;
        _localizationService = localizationService;
        _correlationContextAccessor = correlationContextAccessor;
    }

    private string TrackId { get; set; } = string.Empty;


    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            TrackId = _correlationContextAccessor.CorrelationContext.CorrelationId ?? Guid.NewGuid().ToString();
            await _next(context);
        }
        catch (ApiException exception)
        {
            _logger.LogError(exception, exception.Message);
            await HandleApiExceptionAsync(context, exception);
        }
        catch (ValidationException exception)
        {
            // _logger.LogError(exception, exception.Message);
            await HandleValidationExceptionASync(context, exception);
        }
        catch (InvalidBusinessException exception)
        {
            // _logger.LogError(exception, exception.Message);
            await HandleInValidBusinessExceptionAsync(context, exception);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, exception.Message);
            await HandleInternalServerExceptionAsync(context, exception);
        }
    }

    private async Task HandleInternalServerExceptionAsync(HttpContext context, Exception exception)
    {
        await HandleSystemExceptionAsync(context, HttpStatusCode.InternalServerError, new ExceptionResponse(exception));
    }

    private async Task HandleValidationExceptionASync(HttpContext context, ValidationException exception)
    {
        await HandleValidateClientExceptionAsync(context, HttpStatusCode.BadRequest, new ExceptionResponse(exception));
    }

    private async Task HandleApiExceptionAsync(HttpContext context, ApiException exception)
    {
        await HandleClientExceptionAsync(context, HttpStatusCode.BadRequest, Array.Empty<object>(),
            new ExceptionResponse(exception));
    }

    private async Task HandleInValidBusinessExceptionAsync(HttpContext context, InvalidBusinessException exception)
    {
        await HandleClientExceptionAsync(context, exception.HttpStatusCode, exception.Args,
            new ExceptionResponse(exception));
    }

    private async Task HandleClientExceptionAsync(HttpContext context, HttpStatusCode code, object[] args,
        ExceptionResponse response)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;
        string realMessage = _localizationService.GetMessageErrorString(response.Message, args);
        Error error = new(response.Message, realMessage);
        JsonSerializerOptions options = new()
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
        await context.Response.WriteAsync(JsonSerializer.Serialize(Result.Failure(TrackId, error), options));
    }

    private async Task HandleValidateClientExceptionAsync(HttpContext context, HttpStatusCode code,
        ExceptionResponse response)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        JsonSerializerOptions options = new()
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
        await context.Response.WriteAsync(JsonSerializer.Serialize(Result.Failure
        (response.Details ?? new Dictionary<string, List<string>>(),
            new Error(ResponseCode.ValidationError.GetDescription(), string.Empty), TrackId), options));
    }

    private async Task HandleSystemExceptionAsync(HttpContext context, HttpStatusCode code, ExceptionResponse response)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        JsonSerializerOptions options = new()
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
        await context.Response.WriteAsync(JsonSerializer.Serialize(Result.Failure(TrackId, new Error(
                response.ErrorCode.ToString(),
                response.Details != null ? JsonConvert.SerializeObject(response.Details) : response.Message, true)),
            options));
    }
}

// Extension method for middleware registration
public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseLanguageMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionMiddleware>();
    }
}