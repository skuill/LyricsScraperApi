using FluentValidation;
using LyricsScraperApi.Models.Requests;

namespace LyricsScraperApi.Validators
{
    public class UriSearchRequestValidator : AbstractValidator<UriSearchRequest>
    {
        public UriSearchRequestValidator()
        {
            RuleFor(x => x.Uri).NotNull().WithMessage("The {PropertyName} is not specified in the request.");
        }
    }
}
