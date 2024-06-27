using LyricsScraperApi.Converters;
using LyricsScraperApi.Helpers;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace LyricsScraperApi.Models.Requests
{
    [SwaggerDiscriminator(Constants.SearchRequestDescriminatorName)]
    [SwaggerSubType(typeof(ArtistAndSongSearchRequest), DiscriminatorValue = Constants.ArtistAndSongRequestDescriminatorValue)]
    [SwaggerSubType(typeof(UriSearchRequest), DiscriminatorValue = Constants.UriRequestDescriminatorValue)]
    [JsonConverter(typeof(SearchRequestJsonConverter))]
    public abstract class SearchRequestBase
    {
        /// <summary>
        /// Descriminator value for polymorphism and inheritance.
        /// Available values: ArtistAndSong, Uri.
        /// </summary>
        /// <example>ArtistAndSong</example>
        public string RequestType { get; set; }

        public SearchRequestBase(string requestType)
        {
            RequestType = requestType;
        }
    }
}
