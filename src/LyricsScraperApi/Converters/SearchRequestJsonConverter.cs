using LyricsScraperApi.Helpers;
using LyricsScraperApi.Models.Requests;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LyricsScraperApi.Converters
{
    public sealed class SearchRequestJsonConverter : JsonConverter<SearchRequestBaseDto>
    {
        public override bool CanConvert(Type type)
        {
            return type.IsAssignableFrom(typeof(SearchRequestBaseDto));
        }

        public override SearchRequestBaseDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (JsonDocument.TryParseValue(ref reader, out var doc))
            {
                if (doc.RootElement.TryGetProperty(Constants.SearchRequestDescriminatorName, out var type))
                {
                    var typeValue = type.GetString();
                    var rootElement = doc.RootElement.GetRawText();

                    return typeValue switch
                    {
                        Constants.ArtistAndSongRequestDescriminatorValue => JsonSerializer.Deserialize<ArtistAndSongSearchRequestDto>(rootElement, options),
                        Constants.UriRequestDescriminatorValue => JsonSerializer.Deserialize<UriSearchRequestDto>(rootElement, options),
                        _ => throw new JsonException($"requestType with a value {typeValue} is not a valid!")
                    };
                }

                throw new JsonException("Failed to extract type property, it might be missing?");
            }

            throw new JsonException("Failed to parse JsonDocument");
        }

        public override void Write(Utf8JsonWriter writer, SearchRequestBaseDto value, JsonSerializerOptions options)
        {
            // We don't care about writing JSON.
            throw new NotImplementedException();
        }
    }
}
