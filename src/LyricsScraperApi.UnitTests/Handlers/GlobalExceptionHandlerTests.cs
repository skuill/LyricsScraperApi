using FakeItEasy;
using LyricsScraperApi.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LyricsScraperApi.UnitTests.Handlers
{
    public class GlobalExceptionHandlerTests
    {
        [Fact]
        public async Task TryHandleAsync_ShouldReturnBadRequest_ForArgumentException()
        {
            // Arrange
            var logger = A.Fake<ILogger<GlobalExceptionHandler>>();
            var handler = new GlobalExceptionHandler(logger);
            var context = new DefaultHttpContext();

            var exception = new ArgumentException("Invalid parameter");

            var responseStream = new MemoryStream();
            context.Response.Body = responseStream;

            // Act
            var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);

            responseStream.Seek(0, SeekOrigin.Begin);
            var json = await new StreamReader(responseStream).ReadToEndAsync();
            var problem = JsonSerializer.Deserialize<ProblemDetails>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(problem);
            Assert.Equal("Invalid parameter", problem.Detail);
            Assert.Equal("ArgumentException", problem.Type);
            Assert.Equal(400, problem.Status);

            //A.CallTo(() => logger.Log(
            //    LogLevel.Error,
            //    A<EventId>._,
            //    A<object>._,
            //    exception,
            //    A<Func<object, Exception, string>>._)).MustHaveHappened();
        }

        [Fact]
        public async Task TryHandleAsync_ShouldReturnInternalServerError_ForUnknownException()
        {
            // Arrange
            var logger = A.Fake<ILogger<GlobalExceptionHandler>>();
            var handler = new GlobalExceptionHandler(logger);
            var context = new DefaultHttpContext();

            var exception = new Exception("Something went wrong");

            var responseStream = new MemoryStream();
            context.Response.Body = responseStream;

            // Act
            var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

            responseStream.Seek(0, SeekOrigin.Begin);
            var json = await new StreamReader(responseStream).ReadToEndAsync();
            var problem = JsonSerializer.Deserialize<ProblemDetails>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(problem);
            Assert.Equal("Something went wrong", problem.Detail);
            Assert.Equal("Exception", problem.Type);
            Assert.Equal(500, problem.Status);

            //A.CallTo(() => logger.Log(
            //    LogLevel.Error,
            //    A<EventId>._,
            //    A<object>._,
            //    exception,
            //    A<Func<object, Exception, string>>._)).MustHaveHappened();
        }
    }
}
