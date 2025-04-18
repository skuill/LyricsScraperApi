using LyricsScraperApi.Models.Requests;
using LyricsScraperApi.Models.Responses;
using LyricsScraperApi.ResultPattern;

namespace LyricsScraperApi.Services
{
    public interface ILyricsScraperService
    {
        Task<Result<SearchResultDto>> SearchLyricAsync(SearchRequestBaseDto searchRequestDto);
    }
}
