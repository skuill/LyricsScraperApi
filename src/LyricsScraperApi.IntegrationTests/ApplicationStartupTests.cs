using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using System.Net;
using System.Text.Json;

namespace LyricsScraperApi.IntegrationTests
{
    public class ApplicationStartupTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ApplicationStartupTests(WebApplicationFactory<Program> factory)
        {
            // The Docker image runs in Production.
            _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Production"));
        }

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
            root.GetProperty("paths").TryGetProperty("/api/LyricsScraper", out _).ShouldBeTrue();

            var schemas = root.GetProperty("components").GetProperty("schemas");
            schemas.GetProperty("SearchRequestBaseDto").GetProperty("discriminator").GetProperty("propertyName").GetString()
                .ShouldBe("requestType");
            schemas.GetProperty("ExternalProviderTypeDto").GetProperty("enum").EnumerateArray().Select(value => value.GetString())
                .ShouldBe(["All", "AZLyrics", "Genius", "SongLyrics", "KPopLyrics", "LyricsFreak", "Lrclib"]);
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
}
