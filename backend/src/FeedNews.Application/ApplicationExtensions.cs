using System.Reflection;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using FeedNews.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FeedNews.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationService(this IServiceCollection services, IConfiguration config)
    {
        services.AddEndpointsApiExplorer();

        //Add memory cache
        services.AddMemoryCache();

        //Allow origin
        services.AddCors(opt =>
        {
            opt.AddPolicy("CorsPolicy",
                poli =>
                {
                    poli.WithOrigins("http://localhost:3000,https://level-up-accounts-fe.vercel.app".Split(","))
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
        });

        // MediaR
        Assembly applicationAssembly = typeof(AssemblyReference).Assembly;
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(applicationAssembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });


        // FluentAPI validation
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssembly(applicationAssembly);

        // Fix disable 400 request filter auto
        services.AddControllers()
            .ConfigureApiBehaviorOptions(options => { options.SuppressModelStateInvalidFilter = true; });

        // Config Json Convert
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }
}