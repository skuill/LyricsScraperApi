using LyricsScraperApi.Models.Requests;
using LyricsScraperApi.Validators;

namespace LyricsScraperApi.UnitTests.Validators
{
    public class SearchRequestBaseValidatorTests
    {
        private readonly SearchRequestBaseValidator _validator = new();

        [Fact]
        public void Should_Have_Error_When_RequestType_Is_Empty()
        {
            var model = new ArtistAndSongSearchRequestDto { RequestType = "", Artist = "Artist", Song = "Song" };
            var result = _validator.Validate(model);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "RequestType");
        }

        [Fact]
        public void Should_Have_Error_When_Child_Properties_Invalid()
        {
            var model = new ArtistAndSongSearchRequestDto { RequestType = "artist_song", Artist = "", Song = "" };
            var result = _validator.Validate(model);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Artist");
            Assert.Contains(result.Errors, e => e.PropertyName == "Song");
        }

        [Fact]
        public void Should_Pass_For_Valid_ArtistAndSongSearchRequestDto()
        {
            var model = new ArtistAndSongSearchRequestDto
            {
                RequestType = "artist_song",
                Artist = "Artist",
                Song = "Song"
            };
            var result = _validator.Validate(model);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Pass_For_Valid_UriSearchRequestDto()
        {
            var model = new UriSearchRequestDto
            {
                RequestType = "uri",
                Uri = new Uri("https://example.com/song")
            };
            var result = _validator.Validate(model);
            Assert.True(result.IsValid);
        }
    }
}
