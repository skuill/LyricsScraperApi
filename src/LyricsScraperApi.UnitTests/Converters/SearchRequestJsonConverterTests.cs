using FakeItEasy;
using LyricsScraperApi.Converters;
using LyricsScraperApi.Models;
using LyricsScraperApi.Models.Requests;
using System.Text;
using System.Text.Json;

namespace LyricsScraperApi.UnitTests.Converters
{
    public class SearchRequestJsonConverterTests
    {
        // We'll use our own JsonSerializerOptions instance and register our converter.
        private readonly JsonSerializerOptions _options;

        public SearchRequestJsonConverterTests()
        {
            _options = new JsonSerializerOptions();
            _options.Converters.Add(new SearchRequestJsonConverter());
        }

        private Utf8JsonReader CreateReader(string json)
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            return new Utf8JsonReader(bytes);
        }

        [Fact]
        public void CanConvert_ReturnsTrue_ForSearchRequestBaseType()
        {
            // Arrange
            var converter = new SearchRequestJsonConverter();

            // Act
            var result = converter.CanConvert(typeof(SearchRequestBaseDto));

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Read_ReturnsArtistAndSongSearchRequest_ForValidJson()
        {
            // Arrange
            // Note: Make sure these constant values match those in your Constants class.
            // For example, assume:
            //   Constants.SearchRequestDescriminatorName = "requestType"
            //   Constants.ArtistAndSongRequestDescriminatorValue = "artistAndSong"
            var json = @"{
                ""requestType"": ""ArtistAndSong"",
                ""Artist"": ""Adele"",
                ""Song"": ""Hello""
            }";

            var bytes = Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(bytes);
            var converter = new SearchRequestJsonConverter();

            // Act
            var result = converter.Read(ref reader, typeof(SearchRequestBaseDto), _options);

            // Assert
            Assert.IsType<ArtistAndSongSearchRequestDto>(result);
            var artistRequest = result as ArtistAndSongSearchRequestDto;
            Assert.Equal("Adele", artistRequest.Artist);
            Assert.Equal("Hello", artistRequest.Song);
        }

        [Fact]
        public void Read_ReturnsUriSearchRequest_ForValidJson()
        {
            // Arrange
            // For example, assume:
            //   Constants.SearchRequestDescriminatorName = "requestType"
            //   Constants.UriRequestDescriminatorValue = "uri"
            var json = @"{
                ""requestType"": ""Uri"",
                ""Uri"": ""http://example.com""
            }";

            var bytes = Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(bytes);
            var converter = new SearchRequestJsonConverter();

            // Act
            var result = converter.Read(ref reader, typeof(SearchRequestBaseDto), _options);

            // Assert
            Assert.IsType<UriSearchRequestDto>(result);
            var uriRequest = result as UriSearchRequestDto;
            Assert.Equal(new Uri("http://example.com"), uriRequest.Uri);
            Assert.Equal(ExternalProviderTypeDto.All, uriRequest.Provider);
        }

        [Fact]
        public void Read_ThrowsJsonException_ForMissingDiscriminator()
        {
            // Arrange: JSON without the discriminator property.
            var json = @"{
                ""Artist"": ""Adele"",
                ""Song"": ""Hello""
            }";
            var converter = new SearchRequestJsonConverter();

            // Act & Assert
            var exception = Assert.Throws<JsonException>(() =>
            {
                var bytes = Encoding.UTF8.GetBytes(json);
                var reader = new Utf8JsonReader(bytes);
                converter.Read(ref reader, typeof(SearchRequestBaseDto), _options);
            });

            Assert.Contains("Failed to extract type property", exception.Message);
        }

        [Fact]
        public void Read_ThrowsJsonException_ForUnknownDiscriminatorValue()
        {
            // Arrange: JSON with an unknown discriminator value.
            var json = @"{
                ""requestType"": ""unknown"",
                ""someProperty"": ""value""
            }";

            var converter = new SearchRequestJsonConverter();

            // Act & Assert
            var exception = Assert.Throws<JsonException>(() =>
            {
                var bytes = Encoding.UTF8.GetBytes(json);
                var reader = new Utf8JsonReader(bytes);
                converter.Read(ref reader, typeof(SearchRequestBaseDto), _options);
            });

            Assert.Contains("is not a valid", exception.Message);
        }

        [Fact]
        public void Write_ThrowsNotImplementedException()
        {
            // Arrange: Create a fake instance of SearchRequestBase using FakeItEasy.
            // (Assuming SearchRequestBase is abstract; otherwise, you could instantiate a concrete type.)
            var fakeRequest = A.Fake<SearchRequestBaseDto>();
            var writer = new Utf8JsonWriter(new System.Buffers.ArrayBufferWriter<byte>());
            var converter = new SearchRequestJsonConverter();

            // Act & Assert
            Assert.Throws<NotImplementedException>(() =>
                converter.Write(writer, fakeRequest, _options));
        }
    }
}
