using FeedNews.Application.Common.Repositories;
using FeedNews.Application.Configuration;
using FeedNews.Application.Features.News.Commands;
using FeedNews.Application.Features.News.Queries;
using FeedNews.Domain.Enums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FeedNews.API.BackgroundServices;

public class NewsAggregationBackgroundService : BackgroundService
{
    private readonly ILogger<NewsAggregationBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly NewsConfiguration _newsConfig;
    private Timer? _timer;

    public NewsAggregationBackgroundService(
        ILogger<NewsAggregationBackgroundService> logger,
        IServiceProvider serviceProvider,
        IOptions<NewsConfiguration> newsConfig)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _newsConfig = newsConfig.Value;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("NewsAggregationBackgroundService starting. Configured to run daily at {FetchTime}", _newsConfig.FetchTime);
        
        ScheduleNextExecution();
        
        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("NewsAggregationBackgroundService stopping.");
        
        _timer?.Change(Timeout.Infinite, 0);
        _timer?.Dispose();
        
        await base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    /// <summary>
    /// Schedules the next execution of the news aggregation pipeline at the configured time (18:00).
    /// </summary>
    private void ScheduleNextExecution()
    {
        if (!TimeOnly.TryParse(_newsConfig.FetchTime, out var targetTime))
        {
            _logger.LogError("Invalid FetchTime configuration: {FetchTime}. Using default 18:00", _newsConfig.FetchTime);
            targetTime = new TimeOnly(18, 0);
        }

        var now = DateTime.Now;
        var today = now.Date;
        var scheduledTime = today.Add(targetTime.ToTimeSpan());

        if (scheduledTime <= now)
        {
            scheduledTime = scheduledTime.AddDays(1);
        }

        var delay = scheduledTime - now;

        _logger.LogInformation("Next news aggregation scheduled for {ScheduledTime} (in {DelayMinutes} minutes)",
            scheduledTime, (int)delay.TotalMinutes);

        _timer = new Timer(
            async _ => await ExecuteNewsAggregation(),
            null,
            delay,
            TimeSpan.FromDays(1));
    }

    /// <summary>
    /// Main orchestration method that executes the complete news aggregation pipeline:
    /// 1. Fetch news from both Reuters and VNExpress
    /// 2. Select top 5 articles per category
    /// 3. Generate summaries using Gemini AI
    /// 4. Save to database
    /// 5. Send to Slack
    /// 6. Update slack_sent_at timestamp
    /// </summary>
    private async Task ExecuteNewsAggregation()
    {
        _logger.LogInformation("=== Starting News Aggregation Pipeline ===");

        try
        {
            using (var scope = _serviceProvider.CreateAsyncScope())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<NewsAggregationBackgroundService>>();

                var allFetchedNews = new List<Domain.Entities.News>();
                var newsByCategoryForSlack = new Dictionary<NewsCategory, List<Domain.Entities.News>>();

                foreach (var category in _newsConfig.Categories)
                {
                    if (!Enum.TryParse<NewsCategory>(category, out var newsCategory))
                    {
                        logger.LogWarning("Invalid category configuration: {Category}", category);
                        continue;
                    }

                    logger.LogInformation("Processing category: {Category}", category);

                    await ProcessCategory(mediator, logger, newsCategory, allFetchedNews, newsByCategoryForSlack);
                }

                if (allFetchedNews.Count == 0)
                {
                    logger.LogWarning("No articles were fetched from any category");
                    ScheduleNextExecution();
                    return;
                }

                logger.LogInformation("Successfully processed {ArticleCount} articles across {CategoryCount} categories",
                    allFetchedNews.Count, newsByCategoryForSlack.Count);

                if (newsByCategoryForSlack.Count > 0)
                {
                    await SendToSlack(mediator, logger, newsByCategoryForSlack);
                }

                logger.LogInformation("=== News Aggregation Pipeline Completed Successfully ===");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "=== News Aggregation Pipeline Failed ===");
        }
        finally
        {
            ScheduleNextExecution();
        }
    }

    private async Task ProcessCategory(
        IMediator mediator,
        ILogger logger,
        NewsCategory category,
        List<Domain.Entities.News> allFetchedNews,
        Dictionary<NewsCategory, List<Domain.Entities.News>> newsByCategoryForSlack)
    {
        try
        {
            var fetchCommand = new FetchNewsCommand(category);
            var fetchedNews = await mediator.Send(fetchCommand);

            logger.LogInformation("Fetched {ArticleCount} articles for category {Category}",
                fetchedNews.Count, category);

            if (fetchedNews.Count == 0)
            {
                logger.LogWarning("No articles fetched for category {Category}", category);
                return;
            }

            var rankQuery = new RankAndSelectTopNewsQuery(category, fetchedNews);
            var topNews = await mediator.Send(rankQuery);

            logger.LogInformation("Selected top {TopCount} articles for category {Category}",
                topNews.Count, category);

            var summarizedNews = new List<Domain.Entities.News>();

            foreach (var article in topNews)
            {
                try
                {
                    var summaryCommand = new GenerateSummaryCommand(article);
                    var summarized = await mediator.Send(summaryCommand);
                    summarizedNews.Add(summarized);
                    
                    logger.LogDebug("Generated summary for article: {Title}", article.Title);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to summarize article: {Title}. Using empty summary.", article.Title);
                    article.Summary = string.Empty;
                    summarizedNews.Add(article);
                }
            }

            var unitOfWork = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();
            foreach (var article in summarizedNews)
            {
                await unitOfWork.News.AddAsync(article);
            }
            await unitOfWork.SaveChangesAsync();

            allFetchedNews.AddRange(summarizedNews);
            newsByCategoryForSlack[category] = summarizedNews;

            logger.LogInformation("Completed processing category {Category} with {ArticleCount} articles",
                category, summarizedNews.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing category {Category}", category);
        }
    }

    private async Task SendToSlack(
        IMediator mediator,
        ILogger logger,
        Dictionary<NewsCategory, List<Domain.Entities.News>> newsByCategoryForSlack)
    {
        try
        {
            var allArticles = newsByCategoryForSlack.Values.SelectMany(x => x).ToList();

            var sendCommand = new SendNewsToSlackCommand(allArticles);
            var result = await mediator.Send(sendCommand);

            if (result)
            {
                var unitOfWork = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();
                
                foreach (var article in allArticles)
                {
                    article.SlackSentAt = DateTime.UtcNow;
                    await unitOfWork.News.UpdateAsync(article);
                }
                await unitOfWork.SaveChangesAsync();

                logger.LogInformation("Successfully sent {ArticleCount} articles to Slack", allArticles.Count);
            }
            else
            {
                logger.LogWarning("Failed to send articles to Slack");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending news to Slack");
        }
    }
}
