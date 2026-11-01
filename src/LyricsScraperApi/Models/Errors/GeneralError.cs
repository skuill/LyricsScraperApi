using LyricsScraperApi.Enums;

namespace LyricsScraperApi.Models.Errors
{
    public class GeneralError(string code, string description, StatusCode statusCode)
        : BaseError(code, description, statusCode);
}
