using LyricsScraperApi.Models.Requests;

namespace LyricsScraperApi.Models.Errors
{
    public static class LyricsScraperErrors
    {
        public static BaseError LyricNotFound(SearchRequestBaseDto searchRequest)
            => BaseError.NotFound("Lyric.NotFound", $"Lyric not found. Search request: {searchRequest.ToString()}");

        public static BaseError BadRequestToSearchLyric(SearchRequestBaseDto searchRequest)
            => BaseError.BadRequest("Lyric.BadRequest", $"Lyric not found. Bad search request: {searchRequest.ToString()}");

        public static BaseError LyricSearchError(SearchRequestBaseDto searchRequest, string responseMessage)
            => BaseError.InternalServerError("Lyric.SearchError", $"Lyric not found. Error occured. Search request: {searchRequest.ToString()}. Search response message: {responseMessage}");

        public static BaseError LyricRegionRestricted(SearchRequestBaseDto searchRequest)
            => BaseError.Forbidden("Lyric.RegionRestricted", $"Lyric not found. The lyrics is not available in your region. Search request: {searchRequest.ToString()}.");
    }
}
