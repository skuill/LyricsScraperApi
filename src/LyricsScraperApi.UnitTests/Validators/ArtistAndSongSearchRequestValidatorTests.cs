using LyricsScraperApi.Models.Requests;
using LyricsScraperApi.Validators;

namespace LyricsScraperApi.UnitTests.Validators
{
    public class ArtistAndSongSearchRequestValidatorTests
    {
        private readonly ArtistAndSongSearchRequestValidator _validator = new();

        [Fact]
        public void Should_Have_Error_When_Artist_Is_Empty()
        {
            var model = new ArtistAndSongSearchRequestDto { Artist = "", Song = "Song" };
            var result = _validator.Validate(model);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Artist");
        }

        [Fact]
        public void Should_Have_Error_When_Song_Is_Empty()
        {
            var model = new ArtistAndSongSearchRequestDto { Artist = "Artist", Song = "" };
            var result = _validator.Validate(model);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Song");
        }

        [Fact]
        public void Should_Pass_When_Artist_And_Song_Are_Provided()
        {
            var model = new ArtistAndSongSearchRequestDto { Artist = "Artist", Song = "Song" };
            var result = _validator.Validate(model);
            Assert.True(result.IsValid);
        }
    }
}
