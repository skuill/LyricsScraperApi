using LyricsScraperApi.Mapping;
using LyricsScraperApi.Models;
using LyricsScraperNET.Providers.Models;
using ApiRequests = LyricsScraperApi.Models.Requests;
using LibraryRequests = LyricsScraperNET.Models.Requests;
using LibraryResponses = LyricsScraperNET.Models.Responses;

namespace LyricsScraperApi.UnitTests.Models
{
    public class MappingProfileTests
    {
        [Fact]
        public void MapToLibrary_ArtistAndSongSearchRequest_ShouldMapCorrectly()
        {
            // Arrange
            var apiRequest = new ApiRequests.ArtistAndSongSearchRequestDto
            {
                Artist = "Muse",
                Song = "Uprising",
                Provider = ExternalProviderTypeDto.AZLyrics
            };

            // Act
            var result = apiRequest.MapToLibrary();

            // Assert
            Assert.IsType<LibraryRequests.ArtistAndSongSearchRequest>(result);
            Assert.Equal("Muse", result.Artist);
            Assert.Equal("Uprising", result.Song);
            Assert.Equal(ExternalProviderType.AZLyrics, result.Provider);
        }

        [Fact]
        public void MapToLibrary_UriSearchRequest_ShouldMapCorrectly()
        {
            // Arrange
            var apiRequest = new ApiRequests.UriSearchRequestDto
            {
                Uri = new Uri("https://lyrics.com/test"),
                Provider = ExternalProviderTypeDto.LyricsFreak
            };

            // Act
            var result = apiRequest.MapToLibrary();

            // Assert
            Assert.IsType<LibraryRequests.UriSearchRequest>(result);
            Assert.Equal(new Uri("https://lyrics.com/test"), result.Uri);
            Assert.Equal(ExternalProviderType.LyricsFreak, result.Provider);
        }

        [Fact]
        public void MapToLibrary_SearchRequestBase_ShouldMapArtistAndSongRequest()
        {
            // Arrange
            ApiRequests.SearchRequestBaseDto request = new ApiRequests.ArtistAndSongSearchRequestDto
            {
                Artist = "Radiohead",
                Song = "Creep",
                Provider = ExternalProviderTypeDto.Genius
            };

            // Act
            var result = request.MapToLibrary();

            // Assert
            var typed = Assert.IsType<LibraryRequests.ArtistAndSongSearchRequest>(result);
            Assert.Equal("Radiohead", typed.Artist);
            Assert.Equal("Creep", typed.Song);
            Assert.Equal(ExternalProviderType.Genius, typed.Provider);
        }

        [Fact]
        public void MapToLibrary_SearchRequestBase_UnsupportedType_ShouldThrow()
        {
            // Arrange
            var unknownRequest = new DummyRequest();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => unknownRequest.MapToLibrary());
        }

        [Fact]
        public void MapToApi_SearchResult_ShouldMapCorrectly()
        {
            // Arrange
            var libraryResult = LibraryResponses.SearchResult.Empty;

            // Act
            var result = libraryResult.MapToApi();

            // Assert
            Assert.False(result.Instrumental);
            Assert.Empty(result.LyricText);
        }

        // Dummy unsupported type for testing fallback case
        private class DummyRequest : ApiRequests.SearchRequestBaseDto
        {
            public DummyRequest(string requestType = "DummyRequest") : base(requestType)
            {
            }
        }
    }
}
