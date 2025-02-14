using FakeItEasy;
using LyricsScraperApi.Middlewares;
using LyricsScraperApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;

namespace LyricsScraperApi.UnitTests.Middlewares
{
    public class GlobalExceptionHandlerMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_WhenNextThrowsException_ReturnsErrorResponse()
        {
            // Arrange
            var logger = A.Fake<ILogger<GlobalExceptionHandlerMiddleware>>();
            // Create RequestDelegate that throw an exception
            RequestDelegate next = context => throw new Exception("Test exception");

            var middleware = new GlobalExceptionHandlerMiddleware(logger, next);

            // Create test HttpContext with MemoryStream for Response.Body
            var context = new DefaultHttpContext();
            var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
            Assert.Equal("application/json", context.Response.ContentType);

            // Read response body
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(responseBodyStream, Encoding.UTF8);
            var responseBody = await reader.ReadToEndAsync();

            // Deserialize response
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var errorModel = JsonSerializer.Deserialize<ErrorModel>(responseBody, options);

            Assert.NotNull(errorModel);
            Assert.Equal((int)HttpStatusCode.InternalServerError, errorModel.StatusCode);
            Assert.Equal("Test exception", errorModel.Message);
        }
    }
}
