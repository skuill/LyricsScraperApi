using LyricsScraperApi.Models.Requests;
using LyricsScraperApi.Validators;

namespace LyricsScraperApi.UnitTests.Validators
{
    public class UriSearchRequestValidatorTests
    {
        private readonly UriSearchRequestValidator _validator = new();

        [Fact]
        public void Should_Have_Error_When_Uri_Is_Null()
        {
            var model = new UriSearchRequestDto { Uri = null };
            var result = _validator.Validate(model);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Uri");
        }

        [Fact]
        public void Should_Pass_When_Uri_Is_Provided()
        {
            var model = new UriSearchRequestDto { Uri = new Uri("https://example.com") };
            var result = _validator.Validate(model);
            Assert.True(result.IsValid);
        }
    }
}
