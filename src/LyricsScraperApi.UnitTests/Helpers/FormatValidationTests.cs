using FluentValidation.Results;
using LyricsScraperApi.Helpers;
using Microsoft.AspNetCore.Http;

namespace LyricsScraperApi.UnitTests.Helpers
{
    public class FormatValidationTests
    {
        [Fact]
        public void FormatValidationErrors_WithMultipleFailures_ReturnsFormattedObject()
        {
            // Arrange
            var failures = new List<ValidationFailure>
            {
                new ValidationFailure("Artist", "Artist is required"),
                new ValidationFailure("Song", "Song is required")
            };

            var validationResult = new ValidationResult(failures);
            var formatter = new FormatValidation();

            // Act
            var result = formatter.FormatValidationErrors(validationResult);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Validation Failed", result.Title);
            Assert.Equal(StatusCodes.Status400BadRequest, result.Status);

            Assert.NotNull(result.Errors);
            Assert.Equal(2, result.Errors.Count);

            var firstError = result.Errors[0];
            Assert.Equal("Artist", firstError.Field);
            Assert.Equal("Artist is required", firstError.Message);

            var secondError = result.Errors[1];
            Assert.Equal("Song", secondError.Field);
            Assert.Equal("Song is required", secondError.Message);
        }

        [Fact]
        public void FormatValidationErrors_WithNoFailures_ReturnsEmptyErrorsList()
        {
            // Arrange
            var validationResult = new ValidationResult();
            var formatter = new FormatValidation();

            // Act
            var result = formatter.FormatValidationErrors(validationResult);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Validation Failed", result.Title);
            Assert.Equal(StatusCodes.Status400BadRequest, result.Status);

            Assert.NotNull(result.Errors);
            Assert.Empty(result.Errors);
        }
    }
}
