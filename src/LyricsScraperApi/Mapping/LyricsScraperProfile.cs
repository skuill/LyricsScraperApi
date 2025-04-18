using LyricsScraperApi.Models;
using LyricsScraperNET.Providers.Models;
using ApiRequests = LyricsScraperApi.Models.Requests;
using ApiResponses = LyricsScraperApi.Models.Responses;
using LibraryRequests = LyricsScraperNET.Models.Requests;
using LibraryResponses = LyricsScraperNET.Models.Responses;

namespace LyricsScraperApi.Mapping
{
    public static class LyricsScraperProfile
    {
        // Requests
        public static LibraryRequests.SearchRequest MapToLibrary(this ApiRequests.SearchRequestBaseDto request)
        {
            return request switch
            {
                ApiRequests.ArtistAndSongSearchRequestDto artistReq => artistReq.MapToLibrary(),
                ApiRequests.UriSearchRequestDto uriReq => uriReq.MapToLibrary(),
                _ => throw new NotSupportedException("Unsupported request type")
            };
        }

        public static LibraryRequests.ArtistAndSongSearchRequest MapToLibrary(this ApiRequests.ArtistAndSongSearchRequestDto request)
        {
            return new LibraryRequests.ArtistAndSongSearchRequest
            (
                request.Artist,
                request.Song,
                (ExternalProviderType)request.Provider
            );
        }

        public static LibraryRequests.UriSearchRequest MapToLibrary(this ApiRequests.UriSearchRequestDto request)
        {
            return new LibraryRequests.UriSearchRequest
            (
                request.Uri,
                (ExternalProviderType)request.Provider
            );
        }

        // Responses
        public static ApiResponses.SearchResultDto MapToApi(this LibraryResponses.SearchResult result)
        {
            return new ApiResponses.SearchResultDto
            {
                ExternalProvider = (ExternalProviderTypeDto)result.ExternalProviderType,
                Instrumental = result.Instrumental,
                LyricText = result.LyricText
            };
        }
    }
}
