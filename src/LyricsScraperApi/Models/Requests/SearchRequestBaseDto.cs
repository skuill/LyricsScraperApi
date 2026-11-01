using LyricsScraperApi.Helpers;
using System.Text.Json.Serialization;

namespace LyricsScraperApi.Models.Requests
{
    /// <summary>
    /// A search request. The property <c>requestType</c> selects the kind of search: <c>ArtistAndSong</c> or <c>Uri</c>.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = Constants.SearchRequestDiscriminatorName)]
    [JsonDerivedType(typeof(ArtistAndSongSearchRequestDto), Constants.ArtistAndSongRequestDiscriminatorValue)]
    [JsonDerivedType(typeof(UriSearchRequestDto), Constants.UriRequestDiscriminatorValue)]
    public abstract class SearchRequestBaseDto
    {
    }
}
