using LyricsScraperApi.Models.Requests;

namespace LyricsScraperApi.Models.Errors
{
    public static class LyricsScraperErrors
    {
        public static BaseError LyricNotFound(SearchRequestBaseDto searchRequest)
            => BaseError.NotFound("Lyric.NotFound", $"Lyric not found. Search request: {searchRequest}");

        public static BaseError ProvidersUnavailable(SearchRequestBaseDto searchRequest, string responseMessage)
            => BaseError.ServiceUnavailable("Lyric.ProvidersUnavailable", $"No lyrics provider could answer, try again later. Search request: {searchRequest} Search response message: {responseMessage}");

        public static BaseError BadRequestToSearchLyric(SearchRequestBaseDto searchRequest, string responseMessage)
            => BaseError.BadRequest("Lyric.BadRequest", $"Bad search request: {searchRequest} Search response message: {responseMessage}");

        public static BaseError ServiceMisconfigured(SearchRequestBaseDto searchRequest, string responseMessage)
            => BaseError.InternalServerError("Lyric.ServiceMisconfigured", $"The service has no lyrics providers to search. Search request: {searchRequest} Search response message: {responseMessage}");

        public static BaseError LyricSearchError(SearchRequestBaseDto searchRequest, string resultCode, string responseMessage)
            => BaseError.InternalServerError("Lyric.SearchError", $"Unexpected search result {resultCode}. Search request: {searchRequest} Search response message: {responseMessage}");
    }
}
