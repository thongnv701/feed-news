using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Carter;
using CorrelationId;
using CorrelationId.DependencyInjection;
using FeedNews.API.BackgroundServices;
using FeedNews.API.Middleware;
using FeedNews.Application;
using FeedNews.Domain;
using FeedNews.Infrastructure;
using FeedNews.Infrastructure.Extensions;
using FeedNews.Share;
using Microsoft.OpenApi;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Development.json", false, false);
}
else if (!builder.Environment.IsDevelopment())
{
    string? configurationPath = Environment.GetEnvironmentVariable("ASPNETCORE_CONFIGURATION_PATH");
    if (!string.IsNullOrEmpty(configurationPath) && File.Exists(configurationPath))
        builder.Configuration.SetBasePath(Path.GetDirectoryName(configurationPath) ?? string.Empty)
            .AddJsonFile(Path.GetFileName(configurationPath), false, true);
}

builder.Services.AddDefaultCorrelationId(options =>
{
    options.CorrelationIdGenerator = () => Guid.NewGuid().ToString("N");
    options.AddToLoggingScope = false;
    options.EnforceHeader = false;
    options.IgnoreRequestHeader = false;
    options.IncludeInResponse = true;
    options.RequestHeader = "X-Request-ID";
    options.ResponseHeader = "X-Request-ID";
    options.UpdateTraceIdentifier = false;
});

// Add services to the container.
builder.Services.AddApplicationService(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);
builder.Services.AddDomain();
builder.Services.AddShare();

// Register background service for daily news aggregation
builder.Services.AddHostedService<NewsAggregationBackgroundService>();

builder.Services.AddControllers();

// Logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add Slack webhook logging
var slackWebhookUrl = builder.Configuration["Slack:WebhookUrl"];
if (!string.IsNullOrWhiteSpace(slackWebhookUrl))
{
    var slackMinLogLevel = builder.Configuration["Slack:MinimumLogLevel"] ?? "Warning";
    var includeException = builder.Configuration["Slack:IncludeException"] == "true" || 
                           builder.Configuration["Slack:IncludeException"] == "True";
    var includeTimestamp = builder.Configuration["Slack:IncludeTimestamp"] != "false" && 
                           builder.Configuration["Slack:IncludeTimestamp"] != "False";
    
    if (Enum.TryParse<LogLevel>(slackMinLogLevel, out var logLevel))
    {
        builder.Logging.AddSlackWebhook(
            slackWebhookUrl,
            minimumLogLevel: logLevel,
            includeException: includeException,
            includeTimestamp: includeTimestamp
        );
    }
}

// Carter
builder.Services.AddCarter();
builder.Services.AddApiVersioning(option =>
{
    option.ReportApiVersions = true;
    option.AssumeDefaultVersionWhenUnspecified = true;
    option.DefaultApiVersion = new ApiVersion(1, 0);
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc(
            "v1",
            new OpenApiInfo
            {
                Title = "FeedNews API",
                Version = "v1",
                Description = "API for Supplier Master Data Management",
                Contact = new OpenApiContact { Name = "OPS Tribe" }
            }
        );
        options.CustomSchemaIds(type => type.FullName);
    }
);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    IApiVersionDescriptionProvider apiVersionDescriptionProvider =
        app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwagger(c => { c.RouteTemplate = "/swagger/{documentName}/swagger.json"; });
    app.UseSwaggerUI(options =>
        {
            foreach (ApiVersionDescription description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
            options.RoutePrefix = "swagger";
            options.DefaultModelsExpandDepth(-1);
        }
    );
}

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

app.UseCorrelationId();

app.MapCarter();

app.UseLanguageMiddleware();

app.UseExceptionMiddleware();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
