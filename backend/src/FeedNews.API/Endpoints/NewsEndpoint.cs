using Asp.Versioning;
using Asp.Versioning.Builder;
using Carter;
using FeedNews.Application.Features.News.Commands;
using FeedNews.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NewsEntity = FeedNews.Domain.Entities.News;

namespace FeedNews.API.Endpoints;

public class NewsEndpoint : BaseEndpoint, ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        ApiVersion currentVersion = new(1, 0);
        ApiVersionSet apiVersionSet = app
            .NewApiVersionSet()
            .HasApiVersion(currentVersion)
            .ReportApiVersions()
            .Build();
        RouteGroupBuilder builderGroup = app.MapGroup("api/v{apiVersion:apiVersion}/news")
            .WithApiVersionSet(apiVersionSet);

        // Manual trigger endpoint to test the news aggregation flow
        builderGroup.MapPost("trigger-aggregation/{category}", TriggerNewsAggregation)
            .WithName("TriggerNewsAggregation")
            .WithDescription("Manually trigger news aggregation: fetch, summarize with Gemini AI, and send to Slack")
            .WithOpenApi();
    }

    private static async Task<IResult> TriggerNewsAggregation(
        string category,
        ISender mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            // Parse category
            if (!Enum.TryParse<NewsCategory>(category, true, out var newsCategory))
            {
                return TypedResults.BadRequest(new { 
                    message = "Invalid category",
                    validCategories = Enum.GetNames(typeof(NewsCategory))
                });
            }

            // Step 1: Fetch news from specified category
            var articles = await mediator.Send(new FetchNewsCommand(newsCategory), cancellationToken);
            
            if (!articles.Any())
            {
                return TypedResults.Ok(new { 
                    message = "No articles found for category: " + category,
                    articlesCount = 0
                });
            }

            // Step 2 & 3: Process each article and send to Slack immediately (streaming)
            int processedCount = 0;
            int errorCount = 0;
            var articlesForSlack = new List<NewsEntity>();

            foreach (var article in articles)
            {
                try
                {
                    // Generate summary for this article
                    var summarized = await mediator.Send(new GenerateSummaryCommand(article), cancellationToken);
                    articlesForSlack.Add(summarized);
                    processedCount++;

                    // Send immediately to Slack once we have 3 articles or it's the last one
                    if (articlesForSlack.Count >= 3 || article == articles.Last())
                    {
                        await mediator.Send(new SendNewsToSlackCommand(articlesForSlack), cancellationToken);
                        articlesForSlack.Clear();
                    }
                }
                catch (Exception)
                {
                    errorCount++;
                    // Log but continue processing other articles
                }
            }

            return TypedResults.Ok(new { 
                message = "News aggregation pipeline executed successfully",
                category = category,
                articlesProcessed = processedCount,
                articlesFailed = errorCount,
                steps = new
                {
                    fetched = true,
                    summarizedAndStreamed = true,
                    slackNotified = true
                }
            });
        }
        catch (Exception ex)
        {
            return TypedResults.InternalServerError(new { 
                message = "Error during news aggregation",
                error = ex.Message
            });
        }
    }
}
