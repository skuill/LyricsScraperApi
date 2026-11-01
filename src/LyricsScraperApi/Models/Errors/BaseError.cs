namespace LyricsScraperApi.Models.Errors;

/// <summary>
/// An error of the API: <see cref="Code"/> goes to the <c>errorCode</c> extension of the ProblemDetails response,
/// <see cref="Description"/> to its <c>detail</c>, <see cref="StatusCode"/> is one of <see cref="StatusCodes"/>.
/// </summary>
public sealed class BaseError(string code, string description, int statusCode)
{
    public string Code { get; } = code;
    public string Description { get; } = description;
    public int StatusCode { get; } = statusCode;

    public static BaseError BadRequest(string code, string description)
        => new(code, description, StatusCodes.Status400BadRequest);

    public static BaseError NotFound(string code, string description)
        => new(code, description, StatusCodes.Status404NotFound);

    public static BaseError InternalServerError(string code, string description)
        => new(code, description, StatusCodes.Status500InternalServerError);

    public static BaseError ServiceUnavailable(string code, string description)
        => new(code, description, StatusCodes.Status503ServiceUnavailable);
}
