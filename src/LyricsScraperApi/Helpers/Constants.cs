namespace LyricsScraperApi.Helpers;

public static class Constants
{
    public const string SearchRequestDiscriminatorName = "requestType";
    public const string ArtistAndSongRequestDiscriminatorValue = "ArtistAndSong";
    public const string UriRequestDiscriminatorValue = "Uri";

    /// <summary>
    /// Name of the ProblemDetails extension with the error code of the API, for example <c>Lyric.NotFound</c>.
    /// </summary>
    public const string ErrorCodeExtensionName = "errorCode";
}
