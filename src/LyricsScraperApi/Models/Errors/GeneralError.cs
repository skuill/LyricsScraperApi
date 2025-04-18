using LyricsScraperApi.Enums;

namespace LyricsScraperApi.Models.Errors
{
    public class GeneralError(string title, string description, StatusCode statusCode)
        : BaseError(title, description, statusCode);
}
