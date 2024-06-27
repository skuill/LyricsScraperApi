using AutoMapper;
using ApiRequests = LyricsScraperApi.Models.Requests;
using ApiResponses = LyricsScraperApi.Models.Responses;
using LibraryRequests = LyricsScraperNET.Models.Requests;
using LibraryResponses = LyricsScraperNET.Models.Responses;

namespace LyricsScraperApi.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region requests

            CreateMap<ApiRequests.SearchRequestBase, LibraryRequests.SearchRequest>()
                .Include<ApiRequests.ArtistAndSongSearchRequest, LibraryRequests.ArtistAndSongSearchRequest>()
                .Include<ApiRequests.UriSearchRequest, LibraryRequests.UriSearchRequest>();

            CreateMap<ApiRequests.ArtistAndSongSearchRequest, LibraryRequests.ArtistAndSongSearchRequest>()
                .ForMember(dest => dest.Provider, opt => opt.MapFrom(src => (int)src.Provider));
            CreateMap<ApiRequests.UriSearchRequest, LibraryRequests.UriSearchRequest>()
                .ForMember(dest => dest.Provider, opt => opt.MapFrom(src => (int)src.Provider));

            #endregion

            #region response

            CreateMap<LibraryResponses.SearchResult, ApiResponses.SearchResult>()
                .ForMember(dest => dest.ExternalProvider, opt => opt.MapFrom(src => src.ExternalProviderType))
                .ForMember(dest => dest.Instrumental, opt => opt.MapFrom(src => src.Instrumental))
                .ForMember(dest => dest.LyricTest, opt => opt.MapFrom(src => src.LyricText));

            #endregion
        }
    }
}
