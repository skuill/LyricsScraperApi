namespace LyricsScraperApi.Models.Requests;

/// <summary>
/// Search by the address of a lyrics page.
/// </summary>
public class UriSearchRequestDto : SearchRequestBaseDto
{
    /// <summary>
    /// The web address where the lyrics of one of the supported providers are located.
    /// </summary>
    /// <example>https://genius.com/Parkway-drive-idols-and-anchors-lyrics</example>
    public Uri? Uri { get; set; }

    /// <summary>
    /// The type of external provider for which lyrics will be searched.
    /// By default, it is set to All: the search will be performed across all available client providers.
    /// </summary>
    public ExternalProviderTypeDto Provider { get; set; } = ExternalProviderTypeDto.All;

    public override string ToString()
    {
        return $"Uri: {Uri}. Provider: {Provider}.";
    }
}
