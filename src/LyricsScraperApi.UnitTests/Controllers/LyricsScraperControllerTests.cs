using FakeItEasy;
using LyricsScraperApi.Controllers;
using LyricsScraperApi.Models.Errors;
using LyricsScraperApi.Models.Requests;
using LyricsScraperApi.Models.Responses;
using LyricsScraperApi.ResultPattern;
using LyricsScraperApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LyricsScraperApi.UnitTests.Controllers
{
    public class LyricsScraperControllerTests
    {
        [Fact]
        public async Task GetLyric_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var logger = A.Fake<ILogger<LyricsScraperController>>();
            var service = A.Fake<ILyricsScraperService>();
            var request = new ArtistAndSongSearchRequestDto
            {
                Artist = "Artist",
                Song = "Song"
            };
            var expectedResponse = new SearchResultDto { LyricText = "Some lyrics" };

            A.CallTo(() => service.SearchLyricAsync(request))
                .Returns(Result<SearchResultDto>.Success(expectedResponse));

            var controller = new LyricsScraperController(logger, service);

            // Act
            var result = await controller.GetLyric(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        [Fact]
        public async Task GetLyric_WhenServiceReturnsValidationError_Returns422UnprocessableEntity()
        {
            // Arrange
            var logger = A.Fake<ILogger<LyricsScraperController>>();
            var service = A.Fake<ILyricsScraperService>();
            var request = new ArtistAndSongSearchRequestDto
            {
                Artist = "Artist",
                Song = "Song"
            };

            var error = BaseError.Validation(
                title: "Validation Failed",
                description: "Artist field is required"
            );

            A.CallTo(() => service.SearchLyricAsync(request))
                .Returns(Result<SearchResultDto>.Failure(error));

            var controller = new LyricsScraperController(logger, service);

            // Act
            var result = await controller.GetLyric(request);

            // Assert
            var objectResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
            Assert.Equal(422, objectResult.StatusCode);

            var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
            Assert.Equal("Validation Failed", problemDetails.Title);
            Assert.Equal("Artist field is required", problemDetails.Detail);
            Assert.Equal(422, problemDetails.Status);
        }
    }
}
