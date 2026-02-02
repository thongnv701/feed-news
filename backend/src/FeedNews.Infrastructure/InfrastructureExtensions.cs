using System.Data;
using System.Text;
using System.Text.Json;
using CorrelationId.Abstractions;
using Dapper;
using FeedNews.Application.Common.Repositories;
using FeedNews.Application.Common.Services;
using FeedNews.Application.Common.Services.Dapper;
using FeedNews.Application.Configuration;
using FeedNews.Application.Contracts.Repositories;
using FeedNews.Application.Contracts.Services;
using FeedNews.Application.Shared;
using FeedNews.Domain.Configurations;
using FeedNews.Infrastructure.Common.Data.ApplicationInitialData;
using FeedNews.Infrastructure.Persistence.Contexts;
using FeedNews.Infrastructure.Persistence.Repositories;
using FeedNews.Infrastructure.Services;
using FeedNews.Infrastructure.Services.Dapper;
using FeedNews.Infrastructure.Services.Resource;
using FeedNews.Share.Model.Response;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using StackExchange.Redis;

namespace FeedNews.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration, IHostEnvironment environment)
    {
        #region Initial data

        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        ConnectionStrings? connectionStrings = configuration.GetSection("ConnectionStrings").Get<ConnectionStrings>();
        if (connectionStrings is null)
            throw new ArgumentNullException(nameof(connectionStrings), "ConnectionStrings is null");

        services.AddDbContext<FeedNewsContext>(options =>
        {
            options.UseNpgsql(connectionStrings.FeedNewsDb);
            options.UseSnakeCaseNamingConvention();
        });

        services.AddScoped<ApplicationDbInitializer>();
        using IServiceScope scope = services.BuildServiceProvider().CreateScope();
        ICorrelationContextAccessor correlationContextAccessor =
            scope.ServiceProvider.GetRequiredService<ICorrelationContextAccessor>();
        ApplicationDbInitializer initializer = scope.ServiceProvider.GetRequiredService<ApplicationDbInitializer>();
        if (environment.IsDevelopment())
        {
            // initializer.SeedAsync().Wait();
        }

        #endregion Initial data

        #region Config service

        services.Scan(scan => scan
            .FromAssembliesOf(typeof(BaseService))
            .AddClasses(classes => classes.AssignableTo(typeof(IBaseService)))
            .AsImplementedInterfaces()
            .WithTransientLifetime()
            .FromAssembliesOf(typeof(AccountRepository)) // Infrastructure Layer
            .AddClasses(classes => classes.AssignableTo(typeof(IBaseRepository<>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        #endregion

        #region Dependency Injection

        // Dapper config load underscore
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        services.AddScoped<IDbConnection>(sp => new NpgsqlConnection(
            configuration.GetConnectionString(connectionStrings.FeedNewsDb)));

        // Add unit of work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDapperService, DapperService>();
        services.AddLocalization(options => options.ResourcesPath = "Common/Resources");
        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.AddSingleton<ILanguageAccessor, LanguageAccessor>();

        // Add cache
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(connectionStrings.Redis));
        services.AddStackExchangeRedisCache(option => option.Configuration = connectionStrings.Redis);

        #endregion

        #region Config authentication and jwt bearer

        JwtSettings? jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
        if (jwtSettings is null) throw new ArgumentNullException(nameof(jwtSettings), "JwtSettings is null");

        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(x =>
        {
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            };

            // Customize the response for unauthorized requests
            x.Events = new JwtBearerEvents
            {
                OnChallenge = context =>
                {
                    // Skip the default logic
                    context.HandleResponse();

                    // Set custom response
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    string result = JsonSerializer.Serialize(
                        Result.Failure(correlationContextAccessor.CorrelationContext.CorrelationId,
                            new Error("401", "Authentication failed: JWT token không hợp lệ", true, false)),
                        new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });
                    return context.Response.WriteAsync(result);
                },
                OnForbidden = context =>
                {
                    // Set custom response
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    string result = JsonSerializer.Serialize(
                        Result.Failure(
                            correlationContextAccessor.CorrelationContext.CorrelationId,
                            new Error("403", "Authorization failed: Bạn không có quyền truy cập", true)),
                        new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });
                    return context.Response.WriteAsync(result);
                }
            };
        });
        services.AddAuthorization();

        #endregion

        #region News Aggregation Configuration

        services.AddHttpClient();

        services.Configure<NewsConfiguration>(configuration.GetSection("NewsFeed"));
        services.Configure<GeminiSettings>(configuration.GetSection("Gemini"));
        services.Configure<SlackSettings>(configuration.GetSection("Slack"));
        services.Configure<NewsFeedsConfiguration>(configuration.GetSection("NewsFeeds"));

        services.AddScoped<IReutersNewsService, ReutersNewsService>();
        services.AddScoped<IVNExpressNewsService, VNExpressNewsService>();
        services.AddScoped<IArticleContentFetchService, ArticleContentFetchService>();
        services.AddScoped<IGeminiSummarizationService, GeminiSummarizationService>();
        services.AddScoped<ISlackNotificationService, SlackNotificationService>();
        services.AddScoped<INewsRepository, NewsRepository>();

        #endregion

        return services;
    }
}