using AutoMapper;
using FakeItEasy;
using LyricsScraperApi.Controllers;
using LyricsScraperNET;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LyricsScraperApi.UnitTests.Controllers
{
    public class LyricsScraperControllerTests
    {
        [Fact]
        public async void PostSearchLyric_NullBody_ShouldBe400BadRequest()
        {
            // Arrange
            ILoggerFactory loggerFactory = A.Fake<ILoggerFactory>();
            ILogger<LyricsScraperController> logger = A.Fake<ILogger<LyricsScraperController>>();
            IMapper mapper = A.Fake<IMapper>();
            ILyricsScraperClient lyricsScraperClient = A.Fake<ILyricsScraperClient>();
            var controller = new LyricsScraperController(loggerFactory, logger, mapper, lyricsScraperClient);

            // Act
            var result = await controller.SearchLyric(null);

            // Arrange
            Assert.IsType<BadRequestResult>(result);
        }
    }
}
