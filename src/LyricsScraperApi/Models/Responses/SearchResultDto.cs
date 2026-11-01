namespace LyricsScraperApi.Models.Responses
{
    public class SearchResultDto
    {
        /// <summary>
        /// The flag indicates that the search results are for music only, without text.
        /// </summary>
        public bool Instrumental { get; set; }

        /// <summary>
        /// The type of external provider for which the lyrics were found.
        /// </summary>
        public ExternalProviderTypeDto ExternalProvider { get; set; }

        /// <summary>
        /// The text of the found lyrics. Empty for an instrumental.
        /// </summary>
        public string LyricText { get; set; } = string.Empty;
    }
}
