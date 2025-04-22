using FluentValidation.Results;
using LyricsScraperApi.Models.Errors;

namespace LyricsScraperApi.Helpers;

public interface IFormatValidation
{
    ValidationErrorResponseDto FormatValidationErrors(ValidationResult validationResult);
}

public class FormatValidation : IFormatValidation
{
    public ValidationErrorResponseDto FormatValidationErrors(ValidationResult validationResult)
    {
        return new ValidationErrorResponseDto
        {
            Title = "Validation Failed",
            Status = StatusCodes.Status400BadRequest,
            Errors = validationResult.Errors
                .Select(e => new FieldErrorDto
                {
                    Field = e.PropertyName,
                    Message = e.ErrorMessage
                }).ToList()
        };
    }
}