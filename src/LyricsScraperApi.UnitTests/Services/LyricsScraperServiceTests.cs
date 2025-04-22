using FakeItEasy;
using LyricsScraperApi.Models.Errors;
using LyricsScraperApi.Models.Requests;
using LyricsScraperApi.Services;
using LyricsScraperNET;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Models;
using Microsoft.Extensions.Logging;
using Shouldly;
using System.Reflection;

namespace LyricsScraperApi.UnitTests.Services
{
    public class LyricsScraperServiceTests
    {
        private readonly ILyricsScraperClient _fakeClient;
        private readonly ILogger<LyricsScraperService> _fakeLogger;
        private readonly LyricsScraperService _service;

        public LyricsScraperServiceTests()
        {
            _fakeClient = A.Fake<ILyricsScraperClient>();
            _fakeLogger = A.Fake<ILogger<LyricsScraperService>>();
            _service = new LyricsScraperService(_fakeClient, _fakeLogger);
        }

        [Fact]
        public async Task SearchLyricAsync_WithValidRequest_ReturnsSuccess()
        {
            // Arrange
            string lyricText = "Test lyrics";
            var request = new ArtistAndSongSearchRequestDto { Artist = "Test", Song = "TestSong" };
            var response = CreateSearchResult(responseStatus: ResponseStatusCode.Success, lyricText: lyricText);

            A.CallTo(() => _fakeClient.SearchLyricAsync(A<SearchRequest>.Ignored, default))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _service.SearchLyricAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(lyricText, result.Value.LyricText);
        }

        [Fact]
        public async Task SearchLyricAsync_WhenNoDataFound_ReturnsLyricNotFoundError()
        {
            // Arrange
            var request = new ArtistAndSongSearchRequestDto { Artist = "Test", Song = "Unknown" };
            var response = CreateSearchResult(responseStatus: ResponseStatusCode.NoDataFound);

            A.CallTo(() => _fakeClient.SearchLyricAsync(A<SearchRequest>.Ignored, default))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _service.SearchLyricAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            result.Error.ShouldBeEquivalentTo(LyricsScraperErrors.LyricNotFound(request));
        }

        [Fact]
        public async Task SearchLyricAsync_WhenBadRequest_ReturnsBadRequestError()
        {
            // Arrange
            var request = new ArtistAndSongSearchRequestDto { Artist = "???", Song = "" };
            var response = CreateSearchResult(responseStatus: ResponseStatusCode.BadRequest);

            A.CallTo(() => _fakeClient.SearchLyricAsync(A<SearchRequest>.Ignored, default))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _service.SearchLyricAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            result.Error.ShouldBeEquivalentTo(LyricsScraperErrors.BadRequestToSearchLyric(request));
        }

        [Fact]
        public async Task SearchLyricAsync_WhenError_ReturnsSearchError()
        {
            // Arrange
            string responseMessage = "Internal error";
            var request = new ArtistAndSongSearchRequestDto { Artist = "Test", Song = "ErrorSong" };
            var response = CreateSearchResult(responseStatus: ResponseStatusCode.Error, responseMessage: responseMessage);

            A.CallTo(() => _fakeClient.SearchLyricAsync(A<SearchRequest>.Ignored, default))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _service.SearchLyricAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            result.Error.ShouldBeEquivalentTo(LyricsScraperErrors.LyricSearchError(request, responseMessage));
        }

        [Fact]
        public async Task SearchLyricAsync_WhenRegionRestricted_ReturnsRegionRestrictedError()
        {
            // Arrange
            var request = new ArtistAndSongSearchRequestDto { Artist = "Test", Song = "RestrictedSong" };
            var response = CreateSearchResult(responseStatus: ResponseStatusCode.RegionRestricted);

            A.CallTo(() => _fakeClient.SearchLyricAsync(A<SearchRequest>.Ignored, default))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _service.SearchLyricAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            result.Error.ShouldBeEquivalentTo(LyricsScraperErrors.LyricRegionRestricted(request));
        }

        [Fact]
        public async Task SearchLyricAsync_WhenEmptyResponseAndNotInstrumental_ReturnsLyricNotFound()
        {
            // Arrange
            var request = new ArtistAndSongSearchRequestDto { Artist = "Test", Song = "SilentSong" };
            var response = CreateSearchResult(responseStatus: ResponseStatusCode.Success, lyricText: "", instrumental: false);

            A.CallTo(() => _fakeClient.SearchLyricAsync(A<SearchRequest>.Ignored, default))
                .Returns(Task.FromResult(response));

            // Act
            var result = await _service.SearchLyricAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            result.Error.ShouldBeEquivalentTo(LyricsScraperErrors.LyricNotFound(request));
        }

        public static SearchResult CreateSearchResult(
            string? lyricText = "Some lyrics",
            ResponseStatusCode responseStatus = ResponseStatusCode.Success,
            string? responseMessage = "OK",
            bool instrumental = false)
        {
            var instance = (SearchResult)Activator.CreateInstance(
                typeof(SearchResult),
                nonPublic: true)!;

            SetProperty(instance, "ResponseStatusCode", responseStatus);
            SetProperty(instance, "ResponseMessage", responseMessage);
            SetProperty(instance, "Instrumental", instrumental);
            SetField(instance, "<LyricText>k__BackingField", lyricText);
            SetField(instance, "<ExternalProviderType>k__BackingField", ExternalProviderType.None);

            return instance;
        }

        private static void SetProperty<T>(object target, string propName, T value)
        {
            typeof(SearchResult)
                .GetProperty(propName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!
                .SetValue(target, value);
        }

        private static void SetField<T>(object target, string fieldName, T value)
        {
            typeof(SearchResult)
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(target, value);
        }
    }
}
