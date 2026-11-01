using LyricsScraperApi.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.AspNetCore;
using Serilog.Events;
using Shouldly;
using System.Net;
using System.Text.Json;

namespace LyricsScraperApi.IntegrationTests;

public class ApplicationStartupTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _baseFactory;
    private readonly WebApplicationFactory<Program> _factory;

    public ApplicationStartupTests(WebApplicationFactory<Program> factory)
    {
        _baseFactory = factory;
        // The Docker image runs in Production.
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Production"));
    }

    // Levels from the Serilog section of appsettings.json and appsettings.Development.json.
    [Theory]
    [InlineData("Production", LogLevel.Information)]
    [InlineData("Development", LogLevel.Debug)]
    public void LogLevels_FrameworkOnWarning_ApplicationByEnvironment(string environment, LogLevel applicationLevel)
    {
        // Arrange
        using var factory = _baseFactory.WithWebHostBuilder(builder => builder.UseEnvironment(environment));
        var loggerFactory = factory.Services.GetRequiredService<ILoggerFactory>();
        string[] categories =
        [
            "LyricsScraperApi.Services.LyricsScraperService",
            "Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker",
            "Microsoft.Hosting.Lifetime",
            "System.Net.Http.HttpClient",
            "LyricsScraperNET.Providers.Genius.GeniusProvider",
        ];

        // Act
        var levels = categories.Select(category => $"{category}: {LowestEnabledLevel(loggerFactory.CreateLogger(category))}");

        // Assert
        levels.ShouldBe(
        [
            $"LyricsScraperApi.Services.LyricsScraperService: {applicationLevel}",
            "Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker: Warning",
            "Microsoft.Hosting.Lifetime: Information",
            "System.Net.Http.HttpClient: Warning",
            "LyricsScraperNET.Providers.Genius.GeniusProvider: Error",
        ]);
    }

    // UseSerilogRequestLogging reads its options from DI: health checks below the minimum level, the rest Information.
    [Fact]
    public void RequestLogging_UsesRequestLogLevel()
    {
        // Arrange
        var options = _factory.Services.GetRequiredService<IOptions<RequestLoggingOptions>>().Value;
        var health = new DefaultHttpContext { Request = { Path = RequestLogLevel.HealthPath } };
        var search = new DefaultHttpContext { Request = { Path = "/api/LyricsScraper" } };

        // Act
        LogEventLevel[] levels = [options.GetLevel(health, 1, null), options.GetLevel(search, 1, null)];

        // Assert
        levels.ShouldBe([LogEventLevel.Verbose, LogEventLevel.Information]);
    }

    private static LogLevel LowestEnabledLevel(ILogger logger)
        => Enum.GetValues<LogLevel>().First(level => level == LogLevel.None || logger.IsEnabled(level));

    [Fact]
    public async Task HealthCheckEndpoint_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/health", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OpenApiDocument_InProduction_DescribesTheSearchRequest()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var root = document.RootElement;
        root.GetProperty("info").GetProperty("title").GetString().ShouldBe("LyricsScraperApi");

        var schemas = root.GetProperty("components").GetProperty("schemas");
        schemas.EnumerateObject().Select(schema => schema.Name).ShouldBe(
            ["ExternalProviderType", "ProblemDetails", "SearchRequest", "SearchRequestArtistAndSong", "SearchRequestUri", "SearchResult", "ValidationProblemDetails"],
            ignoreOrder: true);
        schemas.GetProperty("SearchRequest").GetProperty("discriminator").GetProperty("propertyName").GetString().ShouldBe("requestType");
        schemas.GetProperty("SearchRequestArtistAndSong").GetProperty("required").EnumerateArray().Select(field => field.GetString())
            .ShouldBe(["artist", "song"], ignoreOrder: true);
        schemas.GetProperty("SearchRequestUri").GetProperty("required").EnumerateArray().Select(field => field.GetString()).ShouldBe(["uri"]);
        schemas.GetProperty("ExternalProviderType").GetProperty("type").GetString().ShouldBe("string");
        schemas.GetProperty("ExternalProviderType").GetProperty("enum").EnumerateArray().Select(value => value.GetString())
            .ShouldBe(["All", "AZLyrics", "Genius", "SongLyrics", "KPopLyrics", "LyricsFreak", "Lrclib"]);
        schemas.GetProperty("ProblemDetails").GetProperty("properties").EnumerateObject().Select(property => property.Name)
            .ShouldBe(["type", "title", "status", "detail", "instance", "traceId", "errorCode"], ignoreOrder: true);
        schemas.GetProperty("ProblemDetails").GetProperty("properties").GetProperty("status").GetProperty("type").GetString().ShouldBe("integer");

        var search = root.GetProperty("paths").GetProperty("/api/LyricsScraper").GetProperty("post");
        search.GetProperty("operationId").GetString().ShouldBe("SearchLyrics");
        search.GetProperty("requestBody").GetProperty("content").GetProperty("application/json").GetProperty("examples").EnumerateObject()
            .Select(example => example.Value.GetProperty("value").GetProperty("requestType").GetString())
            .ShouldBe(["ArtistAndSong", "Uri"]);
        var responses = search.GetProperty("responses");
        responses.GetProperty("200").GetProperty("content").EnumerateObject().Select(content => content.Name).ShouldBe(["application/json"]);
        string[] errorCodes = ["404", "503"];
        errorCodes.Select(code => responses.GetProperty(code).GetProperty("content").GetProperty("application/problem+json").GetProperty("examples")
                .EnumerateObject().Single().Value.GetProperty("value").GetProperty("errorCode").GetString())
            .ShouldBe(["Lyric.NotFound", "Lyric.ProvidersUnavailable"]);
    }

    [Fact]
    public async Task SwaggerUi_InProduction_IsServed()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/index.html", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("text/html");
    }

    [Fact]
    public async Task Response_WhenClientAcceptsGzip_IsCompressed()
    {
        // Arrange
        var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/openapi/v1.json");
        request.Headers.AcceptEncoding.ParseAdd("gzip");

        // Act
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentEncoding.ShouldBe(["gzip"]);
    }
}
