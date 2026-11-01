using FakeItEasy;
using FluentValidation;
using FluentValidation.Results;
using LyricsScraperApi.Filters;
using LyricsScraperApi.Models.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace LyricsScraperApi.UnitTests.Filters
{
    public class GlobalRequestValidationTests
    {
        private readonly ProblemDetailsFactory _problemDetailsFactory = A.Fake<ProblemDetailsFactory>();

        [Fact]
        public async Task OnActionExecutionAsync_ValidRequest_CallsNext()
        {
            // Arrange
            var validator = A.Fake<IValidator<ArtistAndSongSearchRequestDto>>();
            A.CallTo(() => validator.ValidateAsync(A<ValidationContext<object>>._, A<CancellationToken>._))
                .Returns(new ValidationResult());

            var context = CreateContext(new ArtistAndSongSearchRequestDto { Artist = "Artist", Song = "Song" },
                services => services.AddSingleton(validator));
            var nextCalled = false;

            // Act
            await new GlobalRequestValidation(_problemDetailsFactory).OnActionExecutionAsync(context, () =>
            {
                nextCalled = true;
                return Task.FromResult(new ActionExecutedContext(context, [], controller: null!));
            });

            // Assert
            nextCalled.ShouldBeTrue();
            context.Result.ShouldBeNull();
            A.CallTo(_problemDetailsFactory).MustNotHaveHappened();
        }

        [Fact]
        public async Task OnActionExecutionAsync_InvalidRequest_ReturnsValidationProblemWithJsonFieldNames()
        {
            // Arrange
            var validator = A.Fake<IValidator<ArtistAndSongSearchRequestDto>>();
            A.CallTo(() => validator.ValidateAsync(A<ValidationContext<object>>._, A<CancellationToken>._))
                .Returns(new ValidationResult(
                [
                    new ValidationFailure("Artist", "Artist is required"),
                    new ValidationFailure("Song", "Song is required")
                ]));

            var context = CreateContext(new ArtistAndSongSearchRequestDto(), services => services.AddSingleton(validator));
            var problemDetails = new ValidationProblemDetails();
            ModelStateDictionary? modelState = null;
            A.CallTo(() => _problemDetailsFactory.CreateValidationProblemDetails(
                    context.HttpContext, A<ModelStateDictionary>._, StatusCodes.Status400BadRequest, null, null, null, null))
                .Invokes((HttpContext _, ModelStateDictionary state, int? _, string? _, string? _, string? _, string? _) => modelState = state)
                .Returns(problemDetails);

            // Act
            await new GlobalRequestValidation(_problemDetailsFactory).OnActionExecutionAsync(context, NextMustNotBeCalled);

            // Assert
            context.Result.ShouldBeOfType<BadRequestObjectResult>().Value.ShouldBeSameAs(problemDetails);
            modelState.ShouldNotBeNull();
            modelState
                .OrderBy(e => e.Key)
                .SelectMany(e => e.Value!.Errors.Select(error => $"{e.Key}: {error.ErrorMessage}"))
                .ShouldBe(["artist: Artist is required", "song: Song is required"]);
        }

        [Fact]
        public async Task OnActionExecutionAsync_RequestWithValidatorOfBaseType_UsesBaseValidator()
        {
            // Arrange
            var validator = A.Fake<IValidator<SearchRequestBaseDto>>();
            A.CallTo(() => validator.ValidateAsync(A<ValidationContext<object>>._, A<CancellationToken>._))
                .Returns(new ValidationResult([new ValidationFailure("Uri", "The Uri is not specified in the request.")]));

            var context = CreateContext(new UriSearchRequestDto(), services => services.AddSingleton(validator));
            var problemDetails = new ValidationProblemDetails();
            A.CallTo(() => _problemDetailsFactory.CreateValidationProblemDetails(
                    A<HttpContext>._, A<ModelStateDictionary>._, A<int?>._, A<string?>._, A<string?>._, A<string?>._, A<string?>._))
                .Returns(problemDetails);

            // Act
            await new GlobalRequestValidation(_problemDetailsFactory).OnActionExecutionAsync(context, NextMustNotBeCalled);

            // Assert
            context.Result.ShouldBeOfType<BadRequestObjectResult>().Value.ShouldBeSameAs(problemDetails);
            A.CallTo(() => validator.ValidateAsync(A<ValidationContext<object>>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task OnActionExecutionAsync_RequestWithoutValidator_CallsNext()
        {
            // Arrange
            var context = CreateContext(new UriSearchRequestDto(), _ => { });
            var nextCalled = false;

            // Act
            await new GlobalRequestValidation(_problemDetailsFactory).OnActionExecutionAsync(context, () =>
            {
                nextCalled = true;
                return Task.FromResult(new ActionExecutedContext(context, [], controller: null!));
            });

            // Assert
            nextCalled.ShouldBeTrue();
            context.Result.ShouldBeNull();
        }

        private static ActionExecutingContext CreateContext(object request, Action<IServiceCollection> registerValidators)
        {
            var services = new ServiceCollection();
            registerValidators(services);

            var actionContext = new ActionContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() },
                RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
                ActionDescriptor = new ControllerActionDescriptor(),
            };

            return new ActionExecutingContext(
                actionContext,
                [],
                new Dictionary<string, object?> { { "request", request } },
                controller: null!);
        }

        private static Task<ActionExecutedContext> NextMustNotBeCalled()
            => throw new InvalidOperationException("The action must not run for an invalid request.");
    }
}
