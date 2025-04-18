using LyricsScraperApi.Helpers;

namespace LyricsScraperApi.Models.Requests
{
    public class ArtistAndSongSearchRequestDto : SearchRequestBaseDto
    {
        public ArtistAndSongSearchRequestDto(string requestType = Constants.ArtistAndSongRequestDescriminatorValue) : base(requestType)
        {
        }

        /// <summary>
        /// Artist or band name.
        /// </summary>
        /// <example>Parkway Drive</example>
        public string Artist { get; set; }

        /// <summary>
        /// Song or track title.
        /// </summary>
        /// <example>Idols And Anchors</example>
        public string Song { get; set; }


        /// <summary>
        /// The type of external provider for which lyrics will be searched.
        /// By default, it is set to All - the search will be performed across all available client providers.
        /// </summary>
        public ExternalProvidersDto Provider { get; set; } = ExternalProvidersDto.All;

        public override string ToString()
        {
            return $"Artist: {Artist}. Song: {Song}. Provider: {Provider}.";
        }
    }
}
