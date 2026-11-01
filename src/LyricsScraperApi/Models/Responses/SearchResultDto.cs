namespace LyricsScraperApi.Models.Responses;

public class SearchResultDto
{
    /// <summary>
    /// The flag indicates that the search results are for music only, without text.
    /// </summary>
    /// <example>false</example>
    public bool Instrumental { get; set; }

    /// <summary>
    /// The type of external provider for which the lyrics were found.
    /// </summary>
    /// <example>Lrclib</example>
    public ExternalProviderTypeDto ExternalProvider { get; set; }

    /// <summary>
    /// The text of the found lyrics, lines separated by line breaks. Empty for an instrumental.
    /// </summary>
    /// <example>First line of the lyrics\nSecond line of the lyrics</example>
    public string LyricText { get; set; } = string.Empty;
}
