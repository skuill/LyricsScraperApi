using LyricsScraperApi.Enums;

namespace LyricsScraperApi.Models.Errors
{
    /// <summary>
    /// An error of the API: <see cref="Code"/> goes to the <c>errorCode</c> extension of the ProblemDetails response,
    /// <see cref="Description"/> to its <c>detail</c>.
    /// </summary>
    public abstract class BaseError(string code, string description, StatusCode statusCode)
    {
        public string Code { get; } = code;
        public string Description { get; } = description;
        public StatusCode StatusCode { get; } = statusCode;

        public static BaseError None()
        {
            return new GeneralError(string.Empty, string.Empty, StatusCode.Success);
        }

        public static BaseError BadRequest(string code, string description)
        {
            return new GeneralError(code, description, StatusCode.BadRequest);
        }

        public static BaseError NotFound(string code, string description)
        {
            return new GeneralError(code, description, StatusCode.NotFound);
        }

        public static BaseError Validation(string code, string description)
        {
            return new GeneralError(code, description, StatusCode.Validation);
        }

        public static BaseError Conflict(string code, string description)
        {
            return new GeneralError(code, description, StatusCode.Conflict);
        }

        public static BaseError InternalServerError(string code, string description)
        {
            return new GeneralError(code, description, StatusCode.InternalServerError);
        }

        public static BaseError ServiceUnavailable(string code, string description)
        {
            return new GeneralError(code, description, StatusCode.ServiceUnavailable);
        }

        public static BaseError UnAuthorized(string code, string description)
        {
            return new GeneralError(code, description, StatusCode.UnAuthorized);
        }

        public static BaseError Forbidden(string code, string description)
        {
            return new GeneralError(code, description, StatusCode.Forbidden);
        }
    }
}
