using LyricsScraperApi.Mapping;
using LyricsScraperApi.Models;
using LyricsScraperApi.Models.Responses;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Models;
using Shouldly;
using System.Reflection;
using ApiRequests = LyricsScraperApi.Models.Requests;
using LibraryRequests = LyricsScraperNET.Models.Requests;

namespace LyricsScraperApi.UnitTests.Models;

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
            Provider = ExternalProviderTypeDto.Lrclib
        };

        // Act
        var result = request.MapToLibrary();

        // Assert
        var typed = Assert.IsType<LibraryRequests.ArtistAndSongSearchRequest>(result);
        Assert.Equal("Radiohead", typed.Artist);
        Assert.Equal("Creep", typed.Song);
        Assert.Equal(ExternalProviderType.Lrclib, typed.Provider);
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
    public void MapToApi_FoundResult_ShouldMapCorrectly()
    {
        // Arrange
        var libraryResult = LyricsScraperModelFactory.SearchResult(SearchResultCode.Found, "Some lyrics", ExternalProviderType.Lrclib);

        // Act
        var result = libraryResult.MapToApi();

        // Assert
        result.ShouldBeEquivalentTo(new SearchResultDto
        {
            Instrumental = false,
            ExternalProvider = ExternalProviderTypeDto.Lrclib,
            LyricText = "Some lyrics"
        });
    }

    [Fact]
    public void MapToApi_EmptyResult_ShouldMapCorrectly()
    {
        // Act
        var result = SearchResult.Empty.MapToApi();

        // Assert
        result.ShouldBeEquivalentTo(new SearchResultDto
        {
            Instrumental = false,
            ExternalProvider = ExternalProviderTypeDto.All,
            LyricText = string.Empty
        });
    }

    // The DTO maps to the library enum by number: both must have the same providers with the same numbers.
    [Fact]
    public void ExternalProviderTypeDto_HasTheProvidersOfTheLibrary()
    {
        var libraryProviders = typeof(ExternalProviderType)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.GetCustomAttribute<ObsoleteAttribute>() is null)
            .ToDictionary(field => (int)field.GetValue(null)!, field => field.Name == nameof(ExternalProviderType.None) ? nameof(ExternalProviderTypeDto.All) : field.Name);

        var apiProviders = Enum.GetValues<ExternalProviderTypeDto>()
            .ToDictionary(value => (int)value, value => value.ToString());

        apiProviders.ShouldBeEquivalentTo(libraryProviders);
    }

    // Dummy unsupported type for testing fallback case
    private class DummyRequest : ApiRequests.SearchRequestBaseDto
    {
    }
}
