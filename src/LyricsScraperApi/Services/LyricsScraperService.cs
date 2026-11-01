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
        public async Task<Result<SearchResultDto>> SearchLyricAsync(SearchRequestBaseDto searchRequestDto, CancellationToken cancellationToken)
        {
            logger.LogDebug("Searching lyric by request: {Request}", searchRequestDto);

            var searchRequest = searchRequestDto.MapToLibrary();
            var searchResult = await lyricsScraperClient.SearchLyricAsync(searchRequest, cancellationToken);

            var error = ToError(searchRequestDto, searchResult);
            if (error is not null)
            {
                return Result<SearchResultDto>.Failure(error);
            }

            logger.LogDebug("Successfully searched lyric by request: {Request}", searchRequestDto);
            return Result<SearchResultDto>.Success(searchResult.MapToApi());
        }

        private BaseError? ToError(SearchRequestBaseDto searchRequest, SearchResult searchResult)
        {
            switch (searchResult.Code.Value)
            {
                case SearchResultCode.Values.Found:
                case SearchResultCode.Values.Instrumental:
                    return null;

                case SearchResultCode.Values.NotFound:
                    logger.LogWarning("Lyric not found. Search request: {Request} Providers: {ProviderAttempts}",
                        searchRequest, FormatProviderAttempts(searchResult));
                    return LyricsScraperErrors.LyricNotFound(searchRequest);

                // Every provider failed (site down, timeout, error): the lyrics may exist, the client can retry later.
                case SearchResultCode.Values.AllProvidersFailed:
                    logger.LogError("No lyrics provider could answer. Search request: {Request} Providers: {ProviderAttempts}",
                        searchRequest, FormatProviderAttempts(searchResult));
                    return LyricsScraperErrors.ProvidersUnavailable(searchRequest, searchResult.ResponseMessage);

                case SearchResultCode.Values.EmptyRequest:
                case SearchResultCode.Values.EmptyArtistOrSong:
                case SearchResultCode.Values.EmptyUri:
                case SearchResultCode.Values.ProviderNotAvailable:
                    logger.LogWarning("Bad search request: {Request} Search result: {Code}. Search response message: {Message}",
                        searchRequest, searchResult.Code, searchResult.ResponseMessage);
                    return LyricsScraperErrors.BadRequestToSearchLyric(searchRequest, searchResult.ResponseMessage);

                case SearchResultCode.Values.NoProviders:
                case SearchResultCode.Values.AllProvidersDisabled:
                    logger.LogError("The service has no lyrics providers to search. Search request: {Request} Search result: {Code}",
                        searchRequest, searchResult.Code);
                    return LyricsScraperErrors.ServiceMisconfigured(searchRequest, searchResult.ResponseMessage);

                default:
                    logger.LogError("Unexpected search result {Code}. Search request: {Request} Search response message: {Message}",
                        searchResult.Code, searchRequest, searchResult.ResponseMessage);
                    return LyricsScraperErrors.LyricSearchError(searchRequest, searchResult.Code.Value, searchResult.ResponseMessage);
            }
        }

        private static string FormatProviderAttempts(SearchResult searchResult)
            => string.Join("; ", searchResult.ProviderAttempts);
    }
}
