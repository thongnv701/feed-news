using CorrelationId.DependencyInjection;
using DotNetEnv;
using FeedNews.Application;
using FeedNews.ConsoleApp.Services;
using FeedNews.ConsoleApp.Utils;
using FeedNews.Domain;
using FeedNews.Domain.Enums;
using FeedNews.Infrastructure;
using FeedNews.Share;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

// Load environment variables from .env file if it exists
try
{
    Env.Load();
}
catch
{
    // .env file not found or error loading - continue with environment variables
}

// Build configuration
IConfigurationBuilder configBuilder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add Development config if in development
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
{
    configBuilder.AddJsonFile("appsettings.Development.json", optional: true);
}

IConfigurationRoot configuration = configBuilder.Build();

// Configure dependency injection
var services = new ServiceCollection();

// Add logging
services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();

    // Configure log levels from configuration
    var logConfig = configuration.GetSection("Logging:LogLevel");
    foreach (var item in logConfig.AsEnumerable())
    {
        if (item.Value != null)
        {
            var category = item.Key.Split(':').LastOrDefault() ?? "Default";
            if (Enum.TryParse<LogLevel>(item.Value, out var logLevel))
            {
                loggingBuilder.SetMinimumLevel(logLevel);
            }
        }
    }
});

// Add correlation ID (MUST be before Infrastructure services)
services.AddDefaultCorrelationId(options =>
{
    options.CorrelationIdGenerator = () => Guid.NewGuid().ToString("N");
    options.AddToLoggingScope = false;
    options.EnforceHeader = false;
    options.IgnoreRequestHeader = false;
    options.IncludeInResponse = false; // Not needed for console app
    options.RequestHeader = "X-Request-ID";
    options.ResponseHeader = "X-Request-ID";
    options.UpdateTraceIdentifier = false;
});

// Add application services
services.AddApplicationService(configuration);

// Create a mock IWebHostEnvironment for infrastructure services
var hostEnvironment = new SimpleHostEnvironment 
{ 
    EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production" 
};
services.AddInfrastructureServices(configuration, hostEnvironment);
services.AddDomain();
services.AddShare();

// Register aggregation orchestrator
services.AddScoped<INewsAggregationOrchestrator, NewsAggregationOrchestrator>();

var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

try
{
    // Print header
    ConsoleFormatter.PrintHeader();

    logger.LogInformation("🚀 FeedNews Aggregation Console App started");
    logger.LogInformation("📍 Environment: {Environment}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production");

    // Get the orchestrator
    var orchestrator = serviceProvider.GetRequiredService<INewsAggregationOrchestrator>();

    // Define categories to process
    var categories = new[]
    {
        NewsCategory.Business,
        NewsCategory.Technology,
        NewsCategory.World
    };

    var results = new List<FeedNews.ConsoleApp.Models.AggregationResult>();

    // Execute aggregation for each category
    foreach (var category in categories)
    {
        ConsoleFormatter.PrintCategoryStart(category.ToString());

        try
        {
            var result = await orchestrator.ExecuteAggregationAsync(category);
            results.Add(result);

            if (result.IsSuccess)
            {
                ConsoleFormatter.PrintSuccess($"{category} aggregation completed successfully");
                logger.LogInformation(
                    "✅ {Category}: Fetched={Count}, Summarized={Summary}, TopSelected={Selected}, SlackSent={Slack}",
                    category,
                    result.TotalFetched,
                    result.SummarizedCount,
                    result.TopSelected,
                    result.SlackSent
                );
            }
            else
            {
                ConsoleFormatter.PrintWarning($"{category} aggregation completed with warnings: {result.ErrorMessage}");
                logger.LogWarning("⚠️ {Category}: {Message}", category, result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Unexpected error during {Category} aggregation", category);
            ConsoleFormatter.PrintError($"Unexpected error during {category} aggregation: {ex.Message}");
        }
    }

    // Print summary
    ConsoleFormatter.PrintFooter(results);

    // Log final summary
    var successCount = results.Count(r => r.IsSuccess);
    logger.LogInformation(
        "📊 Aggregation Summary: {Success}/{Total} categories successful, {TotalArticles} articles processed, {TotalSlack} sent to Slack",
        successCount,
        results.Count,
        results.Sum(r => r.TotalFetched),
        results.Sum(r => r.SlackSent)
    );

    // Exit with appropriate code
    if (successCount == results.Count)
    {
        logger.LogInformation("✅ All aggregations completed successfully");
        Environment.Exit(0);
    }
    else if (successCount > 0)
    {
        logger.LogWarning("⚠️ Some aggregations completed with issues");
        Environment.Exit(0); // Exit with 0 as some articles were still processed
    }
    else
    {
        logger.LogError("❌ All aggregations failed");
        Environment.Exit(1);
    }
}
catch (Exception ex)
{
    logger.LogCritical(ex, "💥 Fatal error in aggregation console app");
    ConsoleFormatter.PrintError($"Fatal error: {ex.Message}");
    Console.WriteLine($"\nStack trace:\n{ex.StackTrace}");
    Environment.Exit(1);
}
finally
{
    await serviceProvider.DisposeAsync();
}

// Simple implementation of IWebHostEnvironment for console app
public class SimpleHostEnvironment : IWebHostEnvironment
{
    public string EnvironmentName { get; set; } = "Production";
    public string ApplicationName { get; set; } = "FeedNews.ConsoleApp";
    public string WebRootPath { get; set; } = Directory.GetCurrentDirectory();
    public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
    public IFileProvider WebRootFileProvider { get; set; } = new PhysicalFileProvider(Directory.GetCurrentDirectory());
    public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider(Directory.GetCurrentDirectory());
}
