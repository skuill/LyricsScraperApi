using FluentValidation;
using LyricsScraperApi.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace LyricsScraperApi.Validators
{
    public sealed class SearchRequestValidatorService : ISearchRequestValidatorService
    {
        private readonly IValidator<SearchRequestBase> _searchRequestValidator;

        public SearchRequestValidatorService(
            IValidator<SearchRequestBase> searchRequestValidator)
        {
            _searchRequestValidator = searchRequestValidator
                ?? throw new ArgumentNullException(nameof(searchRequestValidator));
        }

        public async Task<(bool IsSuccess, IActionResult Result)> ValidateRequest(SearchRequestBase searchRequest)
        {
            if (searchRequest == null)
                return (false, new BadRequestObjectResult("The search request is empty."));

            var requestValidationResult = await _searchRequestValidator.ValidateAsync(searchRequest);

            if (!requestValidationResult.IsValid)
            {
                StringBuilder validationErrors = new StringBuilder();
                foreach (var failure in requestValidationResult.Errors)
                {
                    validationErrors.AppendLine("Property " + failure.PropertyName + " failed validation. Error was: " + failure.ErrorMessage);
                }
                return (false, new BadRequestObjectResult(validationErrors.ToString()));
            }
            return (true, null);
        }
    }
}
