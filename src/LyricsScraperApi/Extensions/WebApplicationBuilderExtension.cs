using FluentValidation;
using LyricsScraperApi.Filters;
using LyricsScraperApi.Formatters;
using LyricsScraperApi.Handlers;
using LyricsScraperApi.Services;
using LyricsScraperNET;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Serilog;
using System.Reflection;
using System.Text.Json.Serialization;

namespace LyricsScraperApi.Extensions
{
    public static class WebApplicationBuilderExtension
    {
        public static void AddPresentation(this WebApplicationBuilder builder)
        {
            // Registering the Serilog logger
            builder.Services.AddSerilog((services, loggerConfiguration) =>
            {
                loggerConfiguration.ReadFrom.Configuration(builder.Configuration);
            });

            // Registering the Response Compression
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
            });

            // Registering the exception handler
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Instance =
                        $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

                    var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
                    context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
                };
            });

            // Registering the services
            builder.Services
                .AddControllers(options =>
                {
                    // Register the filter globally
                    options.Filters.AddService<GlobalRequestValidation>();
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    // requestType may come after the other properties of a search request, as in 1.x.
                    options.JsonSerializerOptions.AllowOutOfOrderMetadataProperties = true;
                });
            // PostConfigure: the JSON formatter is added by MVC when the options are built.
            builder.Services.PostConfigure<MvcOptions>(SearchRequestJsonInputFormatter.Replace);

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Info.Title = "LyricsScraperApi";
                    document.Info.Description = "Search for song lyrics across several lyrics sites, built on LyricsScraperNET.";
                    document.Info.Version = Assembly.GetExecutingAssembly()
                        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                        ?? document.Info.Version;
                    return Task.CompletedTask;
                });
            });

            // Registering the Services
            builder.Services.AddScoped<GlobalRequestValidation>();
        }

        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Fluent Validations
            var applicationAssembly = typeof(WebApplicationBuilderExtension).Assembly;
            services.AddValidatorsFromAssembly(applicationAssembly, includeInternalTypes: true);

            // Registering 3rd party Services
            services.AddSingleton<ILyricsScraperClient>(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

                var client = new LyricsScraperClient()
                    .WithAllProviders();
                client.WithLogger(loggerFactory);

                return client;
            });

            // Register app Services
            services.AddScoped<ILyricsScraperService, LyricsScraperService>();

            services.AddHealthChecks();

            return services;
        }
    }
}
