using FakeItEasy;
using LyricsScraperApi.Formatters;
using LyricsScraperApi.Models;
using LyricsScraperApi.Models.Responses;
using LyricsScraperNET;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LyricsScraperApi.IntegrationTests;

/// <summary>
/// The whole pipeline (JSON, validation, errors) with a fake library client instead of the lyrics sites.
/// </summary>
public class LyricsScraperEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string SearchPath = "/api/LyricsScraper";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ILyricsScraperClient _fakeClient = A.Fake<ILyricsScraperClient>();
    private readonly HttpClient _client;

    public LyricsScraperEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Production");
                builder.ConfigureTestServices(services => services.AddSingleton(_fakeClient));
            })
            .CreateClient();
    }

    [Fact]
    public async Task Search_ByArtistAndSong_ReturnsLyric()
    {
        // Arrange
        ArrangeSearchResult(LyricsScraperModelFactory.SearchResult(SearchResultCode.Found, "Lyrics text", ExternalProviderType.Lrclib));

        // Act
        var response = await PostAsync("""{"requestType":"ArtistAndSong","artist":"Parkway Drive","song":"Idols And Anchors","provider":"Lrclib"}""");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SearchResultDto>(JsonOptions, TestContext.Current.CancellationToken);
        body.ShouldBeEquivalentTo(new SearchResultDto
        {
            Instrumental = false,
            ExternalProvider = ExternalProviderTypeDto.Lrclib,
            LyricText = "Lyrics text"
        });
        A.CallTo(() => _fakeClient.SearchLyricAsync(
                A<SearchRequest>.That.Matches(r => r is ArtistAndSongSearchRequest
                    && ((ArtistAndSongSearchRequest)r).Artist == "Parkway Drive"
                    && ((ArtistAndSongSearchRequest)r).Song == "Idols And Anchors"
                    && ((ArtistAndSongSearchRequest)r).Provider == ExternalProviderType.Lrclib),
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Search_RequestTypeAfterOtherProperties_IsAccepted()
    {
        // Arrange
        ArrangeSearchResult(LyricsScraperModelFactory.SearchResult(SearchResultCode.Found, "Lyrics text", ExternalProviderType.Genius));

        // Act
        var response = await PostAsync("""{"uri":"https://genius.com/Parkway-drive-idols-and-anchors-lyrics","requestType":"Uri"}""");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        A.CallTo(() => _fakeClient.SearchLyricAsync(
                A<SearchRequest>.That.Matches(r => r is UriSearchRequest
                    && ((UriSearchRequest)r).Uri == new Uri("https://genius.com/Parkway-drive-idols-and-anchors-lyrics")
                    && ((UriSearchRequest)r).Provider == ExternalProviderType.None),
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Search_NotFound_Returns404ProblemDetails()
    {
        // Arrange
        ArrangeSearchResult(LyricsScraperModelFactory.SearchResult(SearchResultCode.NotFound));

        // Act
        var response = await PostAsync("""{"requestType":"ArtistAndSong","artist":"Parkway Drive","song":"Unknown"}""");

        // Assert
        await ShouldBeProblemAsync(response, HttpStatusCode.NotFound, "Not Found", "Lyric.NotFound",
            "Lyric not found. Search request: Artist: Parkway Drive. Song: Unknown. Provider: All.");
    }

    [Fact]
    public async Task Search_AllProvidersFailed_Returns503ProblemDetails()
    {
        // Arrange
        ArrangeSearchResult(LyricsScraperModelFactory.SearchResult(SearchResultCode.AllProvidersFailed, responseMessage: "All providers failed"));

        // Act
        var response = await PostAsync("""{"requestType":"ArtistAndSong","artist":"Parkway Drive","song":"Wishing Wells"}""");

        // Assert
        await ShouldBeProblemAsync(response, HttpStatusCode.ServiceUnavailable, "Service Unavailable", "Lyric.ProvidersUnavailable",
            "No lyrics provider could answer, try again later. Search request: Artist: Parkway Drive. Song: Wishing Wells. Provider: All. Search response message: All providers failed");
    }

    [Fact]
    public async Task Search_UnexpectedException_Returns500WithoutInternals()
    {
        // Arrange
        A.CallTo(() => _fakeClient.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._))
            .Throws(new InvalidOperationException("Internal details"));

        // Act
        var response = await PostAsync("""{"requestType":"ArtistAndSong","artist":"Parkway Drive","song":"Wishing Wells"}""");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.ShouldNotContain("Internal details");
    }

    // Only the real error of each request: no "The searchRequestBase field is required." next to it.
    [Theory]
    [InlineData("""{"requestType":"ArtistAndSong","artist":"","song":"Wishing Wells"}""", "artist: The Artist is not specified in the request.")]
    [InlineData("""{"requestType":"ArtistAndSong","artist":null,"song":"Wishing Wells"}""", "artist: The Artist is not specified in the request.")]
    [InlineData("""{"requestType":"Uri"}""", "uri: The Uri is not specified in the request.")]
    [InlineData("""{"artist":"Parkway Drive","song":"Wishing Wells"}""", "requestType: " + SearchRequestJsonInputFormatter.MissingRequestTypeMessage)]
    [InlineData("null", ": A non-empty request body is required.")]
    [InlineData("", ": A non-empty request body is required.")]
    public async Task Search_InvalidRequest_Returns400WithOneClearError(string json, string expectedError)
    {
        // Act
        var response = await PostAsync(json);

        // Assert
        (await ErrorsAsync(response)).ShouldBe([expectedError]);
        A.CallTo(_fakeClient).MustNotHaveHappened();
    }

    [Fact]
    public async Task Search_UnknownProvider_Returns400WithAvailableProviders()
    {
        // Act
        var response = await PostAsync("""{"requestType":"ArtistAndSong","artist":"Parkway Drive","song":"Wishing Wells","provider":"Musixmatch"}""");

        // Assert
        (await ErrorsAsync(response)).ShouldBe(
            ["provider: The provider is not valid. Available values: All, AZLyrics, Genius, SongLyrics, KPopLyrics, LyricsFreak, Lrclib."]);
        A.CallTo(_fakeClient).MustNotHaveHappened();
    }

    // The message comes from System.Text.Json; the field is the JSON path.
    [Theory]
    [InlineData("""{"requestType":"Lyrics","artist":"Parkway Drive","song":"Wishing Wells"}""")]
    [InlineData("""{"requestType":"ArtistAndSong","artist":""")]
    public async Task Search_InvalidJson_Returns400ForTheJsonPath(string json)
    {
        // Act
        var response = await PostAsync(json);

        // Assert
        (await ErrorsAsync(response)).Select(error => error.Split(':')[0]).ShouldBe(["$"]);
        A.CallTo(_fakeClient).MustNotHaveHappened();
    }

    // "field: message" for every error of a 400 ValidationProblemDetails response.
    private static async Task<string[]> ErrorsAsync(HttpResponseMessage response)
    {
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        return document.RootElement.GetProperty("errors").EnumerateObject()
            .SelectMany(field => field.Value.EnumerateArray().Select(message => $"{field.Name}: {message.GetString()}"))
            .ToArray();
    }

    private void ArrangeSearchResult(SearchResult searchResult)
    {
        A.CallTo(() => _fakeClient.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._))
            .Returns(searchResult);
    }

    private Task<HttpResponseMessage> PostAsync(string json)
        => _client.PostAsync(SearchPath, new StringContent(json, Encoding.UTF8, "application/json"), TestContext.Current.CancellationToken);

    private static async Task ShouldBeProblemAsync(HttpResponseMessage response, HttpStatusCode status, string title, string errorCode, string detail)
    {
        response.StatusCode.ShouldBe(status);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var problem = document.RootElement;
        problem.GetProperty("status").GetInt32().ShouldBe((int)status);
        problem.GetProperty("title").GetString().ShouldBe(title);
        problem.GetProperty("detail").GetString().ShouldBe(detail);
        problem.GetProperty("errorCode").GetString().ShouldBe(errorCode);
        problem.GetProperty("instance").GetString().ShouldBe($"POST {SearchPath}");
        problem.GetProperty("traceId").GetString().ShouldNotBeNullOrEmpty();
    }
}
