using FakeItEasy;
using FluentValidation;
using FluentValidation.Results;
using LyricsScraperApi.Filters;
using LyricsScraperApi.Helpers;
using LyricsScraperApi.Models.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace LyricsScraperApi.UnitTests.Filters
{
    public class GlobalRequestValidationTests
    {
        [Fact]
        public async Task OnActionExecutionAsync_ValidRequest_CallsNext()
        {
            // Arrange
            var formatValidation = A.Fake<IFormatValidation>();
            var validator = A.Fake<IValidator<ArtistAndSongSearchRequestDto>>();
            var requestDto = new ArtistAndSongSearchRequestDto { Artist = "Artist", Song = "Song" };

            A.CallTo(() => validator.ValidateAsync(A<ValidationContext<object>>._, default))
                .Returns(new ValidationResult());

            var httpContext = new DefaultHttpContext();
            var services = new ServiceCollection();
            services.AddSingleton(typeof(IValidator<ArtistAndSongSearchRequestDto>), validator);
            httpContext.RequestServices = services.BuildServiceProvider();

            var actionContext = new ActionContext
            {
                HttpContext = httpContext,
                RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
                ActionDescriptor = new ControllerActionDescriptor(),
            };

            var actionArguments = new Dictionary<string, object>
            {
                { "request", requestDto }
            };

            var context = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                actionArguments,
                controller: null
            );

            var executedContext = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), controller: null);
            var nextCalled = false;
            Task<ActionExecutedContext> Next() { nextCalled = true; return Task.FromResult(executedContext); }

            var filter = new GlobalRequestValidation(formatValidation);

            // Act
            await filter.OnActionExecutionAsync(context, Next);

            // Assert
            Assert.True(nextCalled);
            Assert.Null(context.Result);
        }

        [Fact]
        public async Task OnActionExecutionAsync_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var formatValidation = A.Fake<IFormatValidation>();
            var validator = A.Fake<IValidator<ArtistAndSongSearchRequestDto>>();
            var requestDto = new ArtistAndSongSearchRequestDto { Artist = "", Song = "" };

            var validationResult = new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("Artist", "Artist is required"),
                new ValidationFailure("Song", "Song is required")
            });

            A.CallTo(() => validator.ValidateAsync(A<ValidationContext<object>>._, default))
                .Returns(validationResult);

            var formattedError = new { Message = "Validation Failed" };
            A.CallTo(() => formatValidation.FormatValidationErrors(validationResult)).Returns(formattedError);

            var httpContext = new DefaultHttpContext();
            var services = new ServiceCollection();
            services.AddSingleton(typeof(IValidator<ArtistAndSongSearchRequestDto>), validator);
            httpContext.RequestServices = services.BuildServiceProvider();

            var actionContext = new ActionContext
            {
                HttpContext = httpContext,
                RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
                ActionDescriptor = new ControllerActionDescriptor(),
            };

            var actionArguments = new Dictionary<string, object>
            {
                { "request", requestDto }
            };

            var context = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                actionArguments,
                controller: null
            );

            var filter = new GlobalRequestValidation(formatValidation);

            // Act
            await filter.OnActionExecutionAsync(context, () => Task.FromResult<ActionExecutedContext>(null));

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(context.Result);
            Assert.Equal(formattedError, badRequestResult.Value);
        }

        [Fact]
        public async Task OnActionExecutionAsync_ValidInheritedRequest_CallsNext()
        {
            // Arrange
            var formatValidation = A.Fake<IFormatValidation>();
            var validator = A.Fake<IValidator<SearchRequestBaseDto>>();
            SearchRequestBaseDto requestDto = new ArtistAndSongSearchRequestDto
            {
                Artist = "Artist",
                Song = "Song"
            };

            A.CallTo(() => validator.ValidateAsync(A<ValidationContext<object>>._, default))
                .Returns(new ValidationResult());

            var httpContext = new DefaultHttpContext();
            var services = new ServiceCollection();
            services.AddSingleton(typeof(IValidator<SearchRequestBaseDto>), validator);
            httpContext.RequestServices = services.BuildServiceProvider();

            var actionContext = new ActionContext
            {
                HttpContext = httpContext,
                RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
                ActionDescriptor = new ControllerActionDescriptor(),
            };

            var actionArguments = new Dictionary<string, object>
            {
                { "request", requestDto }
            };

            var context = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                actionArguments,
                controller: null
            );

            var executedContext = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), controller: null);
            var nextCalled = false;
            Task<ActionExecutedContext> Next() { nextCalled = true; return Task.FromResult(executedContext); }

            var filter = new GlobalRequestValidation(formatValidation);

            // Act
            await filter.OnActionExecutionAsync(context, Next);

            // Assert
            Assert.True(nextCalled);
            Assert.Null(context.Result);
        }

        [Fact]
        public async Task OnActionExecutionAsync_InvalidInheritedRequest_ReturnsBadRequest()
        {
            // Arrange
            var formatValidation = A.Fake<IFormatValidation>();
            var validator = A.Fake<IValidator<SearchRequestBaseDto>>();
            SearchRequestBaseDto requestDto = new UriSearchRequestDto
            {
                Uri = null
            };

            var validationResult = new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("Uri", "The Uri is not specified in the request.")
            });

            A.CallTo(() => validator.ValidateAsync(A<ValidationContext<object>>._, default))
                .Returns(validationResult);

            var formattedError = new { Message = "Validation Failed" };
            A.CallTo(() => formatValidation.FormatValidationErrors(validationResult)).Returns(formattedError);

            var httpContext = new DefaultHttpContext();
            var services = new ServiceCollection();
            services.AddSingleton(typeof(IValidator<SearchRequestBaseDto>), validator);
            httpContext.RequestServices = services.BuildServiceProvider();

            var actionContext = new ActionContext
            {
                HttpContext = httpContext,
                RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
                ActionDescriptor = new ControllerActionDescriptor(),
            };

            var actionArguments = new Dictionary<string, object>
            {
                { "request", requestDto }
            };

            var context = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                actionArguments,
                controller: null
            );

            var filter = new GlobalRequestValidation(formatValidation);

            // Act
            await filter.OnActionExecutionAsync(context, () => Task.FromResult<ActionExecutedContext>(null));

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(context.Result);
            Assert.Equal(formattedError, badRequestResult.Value);
        }
    }
}
