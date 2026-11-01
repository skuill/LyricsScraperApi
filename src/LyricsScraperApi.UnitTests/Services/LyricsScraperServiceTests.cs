using FakeItEasy;
using LyricsScraperApi.Models;
using LyricsScraperApi.Models.Errors;
using LyricsScraperApi.Models.Requests;
using LyricsScraperApi.Models.Responses;
using LyricsScraperApi.Services;
using LyricsScraperNET;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Models;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace LyricsScraperApi.UnitTests.Services;

public class LyricsScraperServiceTests
{
    private readonly ILyricsScraperClient _fakeClient;
    private readonly ILogger<LyricsScraperService> _fakeLogger;
    private readonly LyricsScraperService _service;

    private readonly ArtistAndSongSearchRequestDto _request = new()
    {
        Artist = "Parkway Drive",
        Song = "Idols And Anchors",
        Provider = ExternalProviderTypeDto.Genius
    };

    private static readonly ProviderAttempt[] FailedAttempts =
    [
        LyricsScraperModelFactory.ProviderAttempt(ExternalProviderType.Lrclib, ProviderAttemptOutcome.Failed, message: "502 Bad Gateway"),
        LyricsScraperModelFactory.ProviderAttempt(ExternalProviderType.Genius, ProviderAttemptOutcome.Timeout),
    ];

    public LyricsScraperServiceTests()
    {
        _fakeClient = A.Fake<ILyricsScraperClient>();
        _fakeLogger = A.Fake<ILogger<LyricsScraperService>>();
        _service = new LyricsScraperService(_fakeClient, _fakeLogger);
    }

    [Fact]
    public async Task SearchLyricAsync_WhenFound_ReturnsLyricAndPassesRequestAndToken()
    {
        // Arrange
        using var cancellation = new CancellationTokenSource();
        ArrangeSearchResult(LyricsScraperModelFactory.SearchResult(SearchResultCode.Found, "Test lyrics", ExternalProviderType.Genius));

        // Act
        var result = await _service.SearchLyricAsync(_request, cancellation.Token);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEquivalentTo(new SearchResultDto
        {
            Instrumental = false,
            ExternalProvider = ExternalProviderTypeDto.Genius,
            LyricText = "Test lyrics"
        });
        A.CallTo(() => _fakeClient.SearchLyricAsync(
                A<SearchRequest>.That.Matches(r => r is ArtistAndSongSearchRequest
                    && ((ArtistAndSongSearchRequest)r).Artist == "Parkway Drive"
                    && ((ArtistAndSongSearchRequest)r).Song == "Idols And Anchors"
                    && ((ArtistAndSongSearchRequest)r).Provider == ExternalProviderType.Genius),
                cancellation.Token))
            .MustHaveHappenedOnceExactly();
        ShouldHaveLogged(LogLevel.Information,
            "Lyric found by Genius. Search request: Artist: Parkway Drive. Song: Idols And Anchors. Provider: Genius.");
    }

    [Fact]
    public async Task SearchLyricAsync_WhenInstrumental_ReturnsInstrumentalResult()
    {
        // Arrange
        ArrangeSearchResult(LyricsScraperModelFactory.SearchResult(SearchResultCode.Instrumental, providerType: ExternalProviderType.Lrclib));

        // Act
        var result = await _service.SearchLyricAsync(_request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEquivalentTo(new SearchResultDto
        {
            Instrumental = true,
            ExternalProvider = ExternalProviderTypeDto.Lrclib,
            LyricText = string.Empty
        });
    }

    [Fact]
    public async Task SearchLyricAsync_WhenNotFound_ReturnsLyricNotFoundAndLogsProviders()
    {
        // Arrange
        var attempts = new[]
        {
            LyricsScraperModelFactory.ProviderAttempt(ExternalProviderType.Lrclib, ProviderAttemptOutcome.NotFound),
            LyricsScraperModelFactory.ProviderAttempt(ExternalProviderType.Genius, ProviderAttemptOutcome.NotFound),
        };
        ArrangeSearchResult(LyricsScraperModelFactory.SearchResult(SearchResultCode.NotFound, responseMessage: "No lyrics", providerAttempts: attempts));

        // Act
        var result = await _service.SearchLyricAsync(_request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBeEquivalentTo(LyricsScraperErrors.LyricNotFound(_request));
        ShouldHaveLogged(LogLevel.Information,
            "Lyric not found. Search request: Artist: Parkway Drive. Song: Idols And Anchors. Provider: Genius. Providers: Lrclib: NotFound (0 ms); Genius: NotFound (0 ms)");
    }

    [Fact]
    public async Task SearchLyricAsync_WhenAllProvidersFailed_ReturnsProvidersUnavailableAndLogsProviders()
    {
        // Arrange
        ArrangeSearchResult(LyricsScraperModelFactory.SearchResult(SearchResultCode.AllProvidersFailed, responseMessage: "All providers failed", providerAttempts: FailedAttempts));

        // Act
        var result = await _service.SearchLyricAsync(_request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBeEquivalentTo(LyricsScraperErrors.ProvidersUnavailable(_request, "All providers failed"));
        ShouldHaveLogged(LogLevel.Error,
            "No lyrics provider could answer. Search request: Artist: Parkway Drive. Song: Idols And Anchors. Provider: Genius. Providers: Lrclib: Failed (0 ms): 502 Bad Gateway; Genius: Timeout (0 ms)");
    }

    [Theory]
    [InlineData(SearchResultCode.Values.EmptyRequest)]
    [InlineData(SearchResultCode.Values.EmptyArtistOrSong)]
    [InlineData(SearchResultCode.Values.EmptyUri)]
    [InlineData(SearchResultCode.Values.ProviderNotAvailable)]
    public async Task SearchLyricAsync_WhenRequestIsInvalid_ReturnsBadRequestError(string code)
    {
        // Arrange
        ArrangeSearchResult(LyricsScraperModelFactory.SearchResult(new SearchResultCode(code), responseMessage: "Bad request"));

        // Act
        var result = await _service.SearchLyricAsync(_request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBeEquivalentTo(LyricsScraperErrors.BadRequestToSearchLyric(_request, "Bad request"));
    }

    [Theory]
    [InlineData(SearchResultCode.Values.NoProviders)]
    [InlineData(SearchResultCode.Values.AllProvidersDisabled)]
    public async Task SearchLyricAsync_WhenServiceHasNoProviders_ReturnsServiceMisconfiguredError(string code)
    {
        // Arrange
        ArrangeSearchResult(LyricsScraperModelFactory.SearchResult(new SearchResultCode(code), responseMessage: "No providers"));

        // Act
        var result = await _service.SearchLyricAsync(_request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBeEquivalentTo(LyricsScraperErrors.ServiceMisconfigured(_request, "No providers"));
    }

    [Fact]
    public async Task SearchLyricAsync_WhenCodeIsUnexpected_ReturnsSearchError()
    {
        // Arrange
        ArrangeSearchResult(LyricsScraperModelFactory.SearchResult(SearchResultCode.None, responseMessage: "Internal error"));

        // Act
        var result = await _service.SearchLyricAsync(_request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBeEquivalentTo(LyricsScraperErrors.LyricSearchError(_request, SearchResultCode.Values.None, "Internal error"));
    }

    private void ArrangeSearchResult(SearchResult searchResult)
    {
        A.CallTo(() => _fakeClient.SearchLyricAsync(A<SearchRequest>._, A<CancellationToken>._))
            .Returns(searchResult);
    }

    private void ShouldHaveLogged(LogLevel level, string message)
    {
        A.CallTo(_fakeLogger)
            .Where(call => call.Method.Name == nameof(ILogger.Log)
                && call.GetArgument<LogLevel>(0) == level
                && call.GetArgument<object>(2)!.ToString() == message)
            .MustHaveHappenedOnceExactly();
    }
}
