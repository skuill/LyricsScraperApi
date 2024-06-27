using AutoMapper;
using FakeItEasy;
using FluentValidation;
using LyricsScraperApi.Controllers;
using LyricsScraperApi.Models.Requests;
using LyricsScraperApi.Validators;
using LyricsScraperNET;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LyricsScraperApi.UnitTests.Controllers
{
    public class LyricsScraperControllerTests
    {
        [Fact]
        public async void PostSearchLyric_WithNullRequestBody_ShouldBe400BadRequest()
        {
            // Arrange
            ILoggerFactory loggerFactory = A.Fake<ILoggerFactory>();
            ILogger<LyricsScraperController> logger = A.Fake<ILogger<LyricsScraperController>>();
            IMapper mapper = A.Fake<IMapper>();
            ILyricsScraperClient lyricsScraperClient = A.Fake<ILyricsScraperClient>();
            IValidator<SearchRequestBase> searchRequestValidator = new SearchRequestBaseValidator();
            var controller = new LyricsScraperController(loggerFactory, logger, mapper, lyricsScraperClient, searchRequestValidator);

            // Act
            var result = await controller.SearchLyric(null);

            // Arrange
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async void PostSearchLyric_WithEmptyDescriminatorInRequestBody_ShouldBe400BadRequest(string descriminatorValue)
        {
            // Arrange
            ILoggerFactory loggerFactory = A.Fake<ILoggerFactory>();
            ILogger<LyricsScraperController> logger = A.Fake<ILogger<LyricsScraperController>>();
            IMapper mapper = A.Fake<IMapper>();
            ILyricsScraperClient lyricsScraperClient = A.Fake<ILyricsScraperClient>();
            IValidator<SearchRequestBase> searchRequestValidator = new SearchRequestBaseValidator();
            var controller = new LyricsScraperController(loggerFactory, logger, mapper, lyricsScraperClient, searchRequestValidator);

            SearchRequestBase searchRequest = A.Fake<SearchRequestBase>();
            searchRequest.RequestType = descriminatorValue;

            // Act
            var result = await controller.SearchLyric(searchRequest);

            // Arrange
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData(null, "")]
        [InlineData("", null)]
        [InlineData(null, null)]
        public async void PostSearchLyric_WithMalformedArtistAndSongRequestBody_ShouldBe400BadRequest(string artist, string song)
        {
            // Arrange
            ILoggerFactory loggerFactory = A.Fake<ILoggerFactory>();
            ILogger<LyricsScraperController> logger = A.Fake<ILogger<LyricsScraperController>>();
            IMapper mapper = A.Fake<IMapper>();
            ILyricsScraperClient lyricsScraperClient = A.Fake<ILyricsScraperClient>();
            IValidator<SearchRequestBase> searchRequestValidator = new SearchRequestBaseValidator();
            var controller = new LyricsScraperController(loggerFactory, logger, mapper, lyricsScraperClient, searchRequestValidator);

            var searchRequest = new ArtistAndSongSearchRequest("ArtistAndSong");
            searchRequest.Artist = artist;
            searchRequest.Song = song;

            // Act
            var result = await controller.SearchLyric(searchRequest);

            // Arrange
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async void PostSearchLyric_WithMalformedUriRequestBody_ShouldBe400BadRequest()
        {
            // Arrange
            ILoggerFactory loggerFactory = A.Fake<ILoggerFactory>();
            ILogger<LyricsScraperController> logger = A.Fake<ILogger<LyricsScraperController>>();
            IMapper mapper = A.Fake<IMapper>();
            ILyricsScraperClient lyricsScraperClient = A.Fake<ILyricsScraperClient>();
            IValidator<SearchRequestBase> searchRequestValidator = new SearchRequestBaseValidator();
            var controller = new LyricsScraperController(loggerFactory, logger, mapper, lyricsScraperClient, searchRequestValidator);

            var searchRequest = new UriSearchRequest("Uri");
            searchRequest.Uri = null;

            // Act
            var result = await controller.SearchLyric(searchRequest);

            // Arrange
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
