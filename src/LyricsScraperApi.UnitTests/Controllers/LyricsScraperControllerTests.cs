using FakeItEasy;
using LyricsScraperApi.Controllers;
using LyricsScraperApi.Models.Errors;
using LyricsScraperApi.Models.Requests;
using LyricsScraperApi.Models.Responses;
using LyricsScraperApi.ResultPattern;
using LyricsScraperApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace LyricsScraperApi.UnitTests.Controllers
{
    public class LyricsScraperControllerTests
    {
        private readonly ILyricsScraperService _service = A.Fake<ILyricsScraperService>();
        private readonly ProblemDetailsFactory _problemDetailsFactory = A.Fake<ProblemDetailsFactory>();
        private readonly LyricsScraperController _controller;

        private readonly ArtistAndSongSearchRequestDto _request = new()
        {
            Artist = "Artist",
            Song = "Song"
        };

        public LyricsScraperControllerTests()
        {
            _controller = new LyricsScraperController(A.Fake<ILogger<LyricsScraperController>>(), _service)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
                ProblemDetailsFactory = _problemDetailsFactory
            };
        }

        [Fact]
        public async Task GetLyric_WithValidRequest_ReturnsOkAndPassesToken()
        {
            // Arrange
            using var cancellation = new CancellationTokenSource();
            var expectedResponse = new SearchResultDto { LyricText = "Some lyrics" };

            A.CallTo(() => _service.SearchLyricAsync(_request, cancellation.Token))
                .Returns(Result<SearchResultDto>.Success(expectedResponse));

            // Act
            var result = await _controller.GetLyric(_request, cancellation.Token);

            // Assert
            var okResult = result.ShouldBeOfType<OkObjectResult>();
            okResult.Value.ShouldBeSameAs(expectedResponse);
            A.CallTo(() => _service.SearchLyricAsync(_request, cancellation.Token)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task GetLyric_WhenServiceFails_ReturnsProblemDetailsWithErrorCode()
        {
            // Arrange
            var error = BaseError.ServiceUnavailable("Lyric.ProvidersUnavailable", "No lyrics provider could answer");

            A.CallTo(() => _service.SearchLyricAsync(_request, A<CancellationToken>._))
                .Returns(Result<SearchResultDto>.Failure(error));
            A.CallTo(() => _problemDetailsFactory.CreateProblemDetails(
                    _controller.HttpContext, StatusCodes.Status503ServiceUnavailable, null, null, error.Description, null))
                .Returns(new ProblemDetails
                {
                    Status = StatusCodes.Status503ServiceUnavailable,
                    Title = "Service Unavailable",
                    Detail = error.Description
                });

            // Act
            var result = await _controller.GetLyric(_request, CancellationToken.None);

            // Assert
            var objectResult = result.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe(StatusCodes.Status503ServiceUnavailable);
            objectResult.Value.ShouldBeEquivalentTo(new ProblemDetails
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = "Service Unavailable",
                Detail = error.Description,
                Extensions = { ["errorCode"] = "Lyric.ProvidersUnavailable" }
            });
        }
    }
}
