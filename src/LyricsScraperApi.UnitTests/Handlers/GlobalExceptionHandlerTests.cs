using FakeItEasy;
using LyricsScraperApi.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace LyricsScraperApi.UnitTests.Handlers
{
    public class GlobalExceptionHandlerTests
    {
        private readonly IProblemDetailsService _problemDetailsService = A.Fake<IProblemDetailsService>();
        private ProblemDetailsContext? _writtenContext;

        public GlobalExceptionHandlerTests()
        {
            A.CallTo(() => _problemDetailsService.TryWriteAsync(A<ProblemDetailsContext>._))
                .Invokes((ProblemDetailsContext context) => _writtenContext = context)
                .Returns(true);
        }

        [Fact]
        public async Task TryHandleAsync_ArgumentException_WritesBadRequestWithMessage()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var exception = new ArgumentException("Invalid parameter");

            // Act
            var handled = await CreateHandler(Environments.Production).TryHandleAsync(context, exception, CancellationToken.None);

            // Assert
            handled.ShouldBeTrue();
            context.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
            ShouldHaveWritten(context, exception, new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "An error occurred",
                Detail = "Invalid parameter"
            });
        }

        [Fact]
        public async Task TryHandleAsync_UnknownExceptionInProduction_WritesInternalServerErrorWithoutMessage()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var exception = new Exception("Connection string with a password");

            // Act
            var handled = await CreateHandler(Environments.Production).TryHandleAsync(context, exception, CancellationToken.None);

            // Assert
            handled.ShouldBeTrue();
            context.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
            ShouldHaveWritten(context, exception, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An error occurred",
                Detail = null
            });
        }

        [Fact]
        public async Task TryHandleAsync_UnknownExceptionInDevelopment_WritesInternalServerErrorWithMessage()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var exception = new Exception("Something went wrong");

            // Act
            var handled = await CreateHandler(Environments.Development).TryHandleAsync(context, exception, CancellationToken.None);

            // Assert
            handled.ShouldBeTrue();
            context.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
            ShouldHaveWritten(context, exception, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An error occurred",
                Detail = "Something went wrong"
            });
        }

        [Fact]
        public async Task TryHandleAsync_RequestClosedByClient_Returns499WithoutBody()
        {
            // Arrange
            using var aborted = new CancellationTokenSource();
            aborted.Cancel();
            var context = new DefaultHttpContext { RequestAborted = aborted.Token };

            // Act
            var handled = await CreateHandler(Environments.Production)
                .TryHandleAsync(context, new OperationCanceledException(aborted.Token), CancellationToken.None);

            // Assert
            handled.ShouldBeTrue();
            context.Response.StatusCode.ShouldBe(GlobalExceptionHandler.ClientClosedRequestStatusCode);
            A.CallTo(_problemDetailsService).MustNotHaveHappened();
        }

        [Fact]
        public async Task TryHandleAsync_CanceledWhileClientIsConnected_WritesInternalServerError()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var exception = new OperationCanceledException("Timeout of a provider");

            // Act
            var handled = await CreateHandler(Environments.Production).TryHandleAsync(context, exception, CancellationToken.None);

            // Assert
            handled.ShouldBeTrue();
            context.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
            ShouldHaveWritten(context, exception, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An error occurred",
                Detail = null
            });
        }

        private GlobalExceptionHandler CreateHandler(string environmentName)
        {
            var environment = A.Fake<IHostEnvironment>();
            A.CallTo(() => environment.EnvironmentName).Returns(environmentName);
            return new GlobalExceptionHandler(A.Fake<ILogger<GlobalExceptionHandler>>(), _problemDetailsService, environment);
        }

        private void ShouldHaveWritten(HttpContext httpContext, Exception exception, ProblemDetails expected)
        {
            A.CallTo(() => _problemDetailsService.TryWriteAsync(A<ProblemDetailsContext>._)).MustHaveHappenedOnceExactly();
            _writtenContext.ShouldNotBeNull();
            _writtenContext.HttpContext.ShouldBeSameAs(httpContext);
            _writtenContext.Exception.ShouldBeSameAs(exception);
            _writtenContext.ProblemDetails.ShouldBeEquivalentTo(expected);
        }
    }
}
