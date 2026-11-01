using LyricsScraperApi.Logging;
using Microsoft.AspNetCore.Http;
using Serilog.Events;
using Shouldly;

namespace LyricsScraperApi.UnitTests.Logging;

public class RequestLogLevelTests
{
    [Theory]
    [InlineData("/api/LyricsScraper", StatusCodes.Status200OK, LogEventLevel.Information)]
    [InlineData("/api/LyricsScraper", StatusCodes.Status404NotFound, LogEventLevel.Information)]
    [InlineData("/api/LyricsScraper", StatusCodes.Status503ServiceUnavailable, LogEventLevel.Error)]
    [InlineData("/api/health", StatusCodes.Status200OK, LogEventLevel.Verbose)]
    [InlineData("/api/health", StatusCodes.Status503ServiceUnavailable, LogEventLevel.Error)]
    public void Get_ByPathAndStatus_ReturnsLevel(string path, int statusCode, LogEventLevel expected)
    {
        // Arrange
        var context = new DefaultHttpContext { Request = { Path = path }, Response = { StatusCode = statusCode } };

        // Act
        var level = RequestLogLevel.Get(context, elapsedMilliseconds: 10, exception: null);

        // Assert
        level.ShouldBe(expected);
    }

    [Fact]
    public void Get_WithException_ReturnsError()
    {
        // Arrange
        var context = new DefaultHttpContext { Request = { Path = "/api/health" }, Response = { StatusCode = StatusCodes.Status200OK } };

        // Act
        var level = RequestLogLevel.Get(context, elapsedMilliseconds: 10, exception: new InvalidOperationException());

        // Assert
        level.ShouldBe(LogEventLevel.Error);
    }
}
