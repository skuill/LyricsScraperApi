using FluentValidation;
using LyricsScraperApi.Filters;
using LyricsScraperApi.Helpers;
using LyricsScraperApi.Handlers;
using LyricsScraperApi.Models.Requests;
using LyricsScraperApi.Services;
using LyricsScraperNET;
using Microsoft.AspNetCore.Http.Features;
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
            builder.Host.UseSerilog((context, loggerConfiguration) =>
            {
                loggerConfiguration.ReadFrom.Configuration(context.Configuration);
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
                });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(opts =>
            {
                opts.EnableAnnotations(enableAnnotationsForInheritance: true, enableAnnotationsForPolymorphism: true);
                opts.UseOneOfForPolymorphism();
                opts.SelectSubTypesUsing(baseType =>
                {
                    if (baseType == typeof(SearchRequestBaseDto))
                    {
                        return new[]
                        {
                typeof(ArtistAndSongSearchRequestDto),
                typeof(UriSearchRequestDto),
                };
                    }

                    return Enumerable.Empty<Type>();
                });

                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                opts.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

            builder.Services.AddHttpContextAccessor();

            // Registering the Services
            builder.Services.AddScoped<GlobalRequestValidation>();
            builder.Services.AddScoped<IFormatValidation, FormatValidation>();
        }

        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var applicationAssembly = typeof(WebApplicationBuilderExtension).Assembly;

            // Fluent Validations
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
