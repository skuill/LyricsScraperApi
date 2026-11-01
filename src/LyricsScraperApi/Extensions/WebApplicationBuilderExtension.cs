using FluentValidation;
using LyricsScraperApi.Filters;
using LyricsScraperApi.Formatters;
using LyricsScraperApi.Handlers;
using LyricsScraperApi.Logging;
using LyricsScraperApi.OpenApi;
using LyricsScraperApi.Services;
using LyricsScraperNET;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Serilog;
using Serilog.AspNetCore;
using System.Text.Json.Serialization;

namespace LyricsScraperApi.Extensions;

public static class WebApplicationBuilderExtension
{
    public static void AddPresentation(this WebApplicationBuilder builder)
    {
        // Registering the Serilog logger
        builder.Services.AddSerilog((services, loggerConfiguration) =>
        {
            loggerConfiguration.ReadFrom.Configuration(builder.Configuration);
        });

        // Read by app.UseSerilogRequestLogging(): quiet health checks, errors as Error.
        builder.Services.Configure<RequestLoggingOptions>(options => options.GetLevel = RequestLogLevel.Get);

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
                // FluentValidation checks the requests. The implicit [Required] of non-nullable reference types would
                // only add "The searchRequestBase field is required." next to the real JSON error. An empty body or
                // the JSON null is still a 400 ("A non-empty request body is required.").
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
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
            options.CreateSchemaReferenceId = SchemaReferenceIds.Create;
            options.AddDocumentTransformer<DocumentInfoTransformer>();
            options.AddOperationTransformer<SearchExamplesOperationTransformer>();
            options.AddSchemaTransformer<EnumSchemaTransformer>();
            options.AddSchemaTransformer<ProblemDetailsSchemaTransformer>();
            options.AddSchemaTransformer<ValidatorRequiredSchemaTransformer>();
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
