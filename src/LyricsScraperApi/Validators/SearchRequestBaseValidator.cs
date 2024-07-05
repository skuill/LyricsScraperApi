using FluentValidation;
using LyricsScraperApi.Models.Requests;

namespace LyricsScraperApi.Validators
{
    public class SearchRequestBaseValidator : AbstractValidator<SearchRequestBase>
    {
        public SearchRequestBaseValidator()
        {
            RuleFor(x => x.RequestType).NotEmpty().WithMessage("The descriminator {PropertyName} is not specified in the request.");
            RuleFor(x => x).SetInheritanceValidator(v =>
            {
                v.Add<ArtistAndSongSearchRequest>(new ArtistAndSongSearchRequestValidator());
                v.Add<UriSearchRequest>(new UriSearchRequestValidator());
            });
        }
    }
}
