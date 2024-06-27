namespace LyricsScraperApi.Models.Responses
{
    public class SearchResult
    {
        /// <summary>
        /// The flag indicates that the search results are for music only, without text.
        /// </summary>
        public bool Instrumental { get; set; }

        /// <summary>
        /// The type of external provider for which the lyrics were found.
        /// </summary>
        public string ExternalProvider { get; set; }

        /// <summary>
        /// The text of the found lyrics. If the lyrics could not be found, an empty value is returned.
        /// </summary>
        public string LyricTest { get; set; }
    }
}
