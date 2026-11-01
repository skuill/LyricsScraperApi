using System.Text.Json.Serialization;

namespace LyricsScraperApi.Models
{
    /// <summary>
    /// Lyrics provider. The numbers are the same as in <c>LyricsScraperNET.Providers.Models.ExternalProviderType</c>
    /// (3 and 5 were Musixmatch and LyricFind, removed in LyricsScraperNET 3.0.0), so a value maps by number.
    /// </summary>
    // On the type, not only in the MVC options: the OpenAPI document reads the converter from here and lists the names.
    [JsonConverter(typeof(JsonStringEnumConverter<ExternalProviderTypeDto>))]
    public enum ExternalProviderTypeDto
    {
        /// <summary>
        /// Every provider in a request; no provider in a response without lyrics.
        /// </summary>
        All = 0,
        AZLyrics = 1,
        Genius = 2,
        SongLyrics = 4,
        KPopLyrics = 6,
        LyricsFreak = 7,
        Lrclib = 8
    }
}
