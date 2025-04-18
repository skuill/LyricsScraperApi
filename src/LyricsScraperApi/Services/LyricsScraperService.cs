using LyricsScraperApi.Mapping;
using LyricsScraperApi.Models.Errors;
using LyricsScraperApi.Models.Requests;
using LyricsScraperApi.Models.Responses;
using LyricsScraperApi.ResultPattern;
using LyricsScraperNET;
using LyricsScraperNET.Models.Responses;

namespace LyricsScraperApi.Services
{
    public class LyricsScraperService(
        ILyricsScraperClient lyricsScraperClient,
        ILogger<LyricsScraperService> logger) : ILyricsScraperService
    {
        public async Task<Result<SearchResultDto>> SearchLyricAsync(SearchRequestBaseDto searchRequestDto)
        {
            logger.LogDebug("Searching lyric by request: {Request}", searchRequestDto.ToString());

            var searchRequest = searchRequestDto.MapToLibrary();
            var searchResult = await lyricsScraperClient.SearchLyricAsync(searchRequest);

            var validationResult = await ValidateSearchResult(searchRequestDto, searchResult);
            if (!validationResult.IsSuccess)
            {
                return Result<SearchResultDto>.Failure(validationResult.Error!);
            }

            var searchResultDto = searchResult.MapToApi();

            logger.LogDebug("Successfully searched lyric by request: {Request}", searchRequestDto.ToString());
            return Result<SearchResultDto>.Success(searchResultDto);
        }

        private async Task<(bool IsSuccess, BaseError? Error)> ValidateSearchResult(SearchRequestBaseDto searchRequest, SearchResult? searchResult)
        {
            if (searchResult is null
                || (searchResult.IsEmpty() && !searchResult.Instrumental)
                || searchResult.ResponseStatusCode == ResponseStatusCode.NoDataFound)
            {
                logger.LogWarning($"Lyric not found. Search request: {searchRequest.ToString()}");
                return (false, LyricsScraperErrors.LyricNotFound(searchRequest));
            }

            if (searchResult.ResponseStatusCode == ResponseStatusCode.BadRequest)
            {
                logger.LogError($"Lyric not found. Bad search request: {searchRequest.ToString()}");
                return (false, LyricsScraperErrors.BadRequestToSearchLyric(searchRequest));
            }

            if (searchResult.ResponseStatusCode == ResponseStatusCode.Error)
            {
                logger.LogError($"Lyric not found. Error occured. Search request: {searchRequest.ToString()}. Search response message: {searchResult!.ResponseMessage}");
                return (false, LyricsScraperErrors.LyricSearchError(searchRequest, searchResult.ResponseMessage));
            }

            if (searchResult.ResponseStatusCode == ResponseStatusCode.RegionRestricted)
            {
                logger.LogInformation($"Lyric not found. The lyrics is not available in your region. Search request: {searchRequest.ToString()}.");
                return (false, LyricsScraperErrors.LyricRegionRestricted(searchRequest));
            }

            return (true, null);
        }
    }
}
