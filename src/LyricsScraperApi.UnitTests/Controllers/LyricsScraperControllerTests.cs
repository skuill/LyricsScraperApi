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
            var controller = GetLyricsScraperControllerFake();

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
            var controller = GetLyricsScraperControllerFake();

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
            var controller = GetLyricsScraperControllerFake();

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
            var controller = GetLyricsScraperControllerFake();

            var searchRequest = new UriSearchRequest("Uri");
            searchRequest.Uri = null;

            // Act
            var result = await controller.SearchLyric(searchRequest);

            // Arrange
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #region Helpers

        private LyricsScraperController GetLyricsScraperControllerFake()
        {
            ILogger<LyricsScraperController> logger = A.Fake<ILogger<LyricsScraperController>>();
            IMapper mapper = A.Fake<IMapper>();
            ILyricsScraperClient lyricsScraperClient = A.Fake<ILyricsScraperClient>();
            IValidator<SearchRequestBase> searchRequestValidator = new SearchRequestBaseValidator();
            ISearchRequestValidatorService searchRequestValidatorService = new SearchRequestValidatorService(searchRequestValidator);

            var controller = new LyricsScraperController(logger, mapper, lyricsScraperClient, searchRequestValidatorService);

            return controller;
        }

        #endregion
    }
}
