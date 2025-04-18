using FakeItEasy;
using LyricsScraperApi.Controllers;
using LyricsScraperApi.Models.Requests;
using LyricsScraperApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LyricsScraperApi.UnitTests.Controllers
{
    public class LyricsScraperControllerTests
    {
        [Fact]
        public async void PostGetLyric_WithNullRequestBody_ShouldBe400BadRequest()
        {
            // Arrange
            var controller = GetLyricsScraperControllerFake();

            // Act
            var result = await controller.GetLyric(null);

            // Arrange
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async void PostGetLyric_WithEmptyDescriminatorInRequestBody_ShouldBe400BadRequest(string descriminatorValue)
        {
            // Arrange
            var controller = GetLyricsScraperControllerFake();

            SearchRequestBaseDto searchRequest = A.Fake<SearchRequestBaseDto>();
            searchRequest.RequestType = descriminatorValue;

            // Act
            var result = await controller.GetLyric(searchRequest);

            // Arrange
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData(null, "")]
        [InlineData("", null)]
        [InlineData(null, null)]
        public async void PostGetLyric_WithMalformedArtistAndSongRequestBody_ShouldBe400BadRequest(string artist, string song)
        {
            // Arrange
            var controller = GetLyricsScraperControllerFake();

            var searchRequest = new ArtistAndSongSearchRequestDto();
            searchRequest.Artist = artist;
            searchRequest.Song = song;

            // Act
            var result = await controller.GetLyric(searchRequest);

            // Arrange
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async void PostGetLyric_WithMalformedUriRequestBody_ShouldBe400BadRequest()
        {
            // Arrange
            var controller = GetLyricsScraperControllerFake();

            var searchRequest = new UriSearchRequestDto();
            searchRequest.Uri = null;

            // Act
            var result = await controller.GetLyric(searchRequest);

            // Arrange
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #region Helpers

        private LyricsScraperController GetLyricsScraperControllerFake()
        {
            ILogger<LyricsScraperController> logger = A.Fake<ILogger<LyricsScraperController>>();
            ILyricsScraperService lyricsScraperService = A.Fake<ILyricsScraperService>(); ;

            var controller = new LyricsScraperController(logger, lyricsScraperService);

            return controller;
        }

        #endregion
    }
}
