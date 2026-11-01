namespace LyricsScraperApi.Models.Requests
{
    /// <summary>
    /// Search by artist and song title.
    /// </summary>
    public class ArtistAndSongSearchRequestDto : SearchRequestBaseDto
    {
        /// <summary>
        /// Artist or band name.
        /// </summary>
        /// <example>Parkway Drive</example>
        public string Artist { get; set; } = string.Empty;

        /// <summary>
        /// Song or track title.
        /// </summary>
        /// <example>Idols And Anchors</example>
        public string Song { get; set; } = string.Empty;

        /// <summary>
        /// The type of external provider for which lyrics will be searched.
        /// By default, it is set to All: the search will be performed across all available client providers.
        /// </summary>
        public ExternalProviderTypeDto Provider { get; set; } = ExternalProviderTypeDto.All;

        public override string ToString()
        {
            return $"Artist: {Artist}. Song: {Song}. Provider: {Provider}.";
        }
    }
}
