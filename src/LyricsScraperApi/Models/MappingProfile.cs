using LyricsScraperNET.Providers.Models;
using ApiRequests = LyricsScraperApi.Models.Requests;
using ApiResponses = LyricsScraperApi.Models.Responses;
using LibraryRequests = LyricsScraperNET.Models.Requests;
using LibraryResponses = LyricsScraperNET.Models.Responses;

namespace LyricsScraperApi.Models
{
    public static class MappingProfile
    {
        // Requests
        public static LibraryRequests.SearchRequest MapToLibrary(this ApiRequests.SearchRequestBase request)
        {
            return request switch
            {
                ApiRequests.ArtistAndSongSearchRequest artistReq => artistReq.MapToLibrary(),
                ApiRequests.UriSearchRequest uriReq => uriReq.MapToLibrary(),
                _ => throw new NotSupportedException("Unsupported request type")
            };
        }

        public static LibraryRequests.ArtistAndSongSearchRequest MapToLibrary(this ApiRequests.ArtistAndSongSearchRequest request)
        {
            return new LibraryRequests.ArtistAndSongSearchRequest
            (
                request.Artist,
                request.Song,
                (ExternalProviderType)request.Provider
            );
        }

        public static LibraryRequests.UriSearchRequest MapToLibrary(this ApiRequests.UriSearchRequest request)
        {
            return new LibraryRequests.UriSearchRequest
            (
                request.Uri,
                (ExternalProviderType)request.Provider
            );
        }

        // Responses
        public static ApiResponses.SearchResult MapToApi(this LibraryResponses.SearchResult result)
        {
            return new ApiResponses.SearchResult
            {
                ExternalProvider = result.ExternalProviderType.ToString(),
                Instrumental = result.Instrumental,
                LyricText = result.LyricText
            };
        }
    }
}
