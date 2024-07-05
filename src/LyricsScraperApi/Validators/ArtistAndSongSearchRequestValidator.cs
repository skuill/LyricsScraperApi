using FluentValidation;
using LyricsScraperApi.Models.Requests;

namespace LyricsScraperApi.Validators
{
    public class ArtistAndSongSearchRequestValidator : AbstractValidator<ArtistAndSongSearchRequest>
    {
        public ArtistAndSongSearchRequestValidator()
        {
            RuleFor(x => x.Artist).NotEmpty().WithMessage("The {PropertyName} is not specified in the request.");
            RuleFor(x => x.Song).NotEmpty().WithMessage("The {PropertyName} is not specified in the request.");
        }
    }
}
